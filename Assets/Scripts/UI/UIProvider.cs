using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.UI.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace ROC.UI
{
	public class UIProvider : IUIProvider, IInitializable, IDisposable
	{
		private readonly Dictionary<Type, IPresenter<IView>> _activePresenters = new();
		private readonly Dictionary<UILayer, List<IPresenter<IView>>> _layerPresenters = new();
		private readonly Dictionary<UILayer, Transform> _layerRoots = new();
		private readonly ILoggingService _logger;
		private readonly IEventBus _eventBus;
		private readonly IAssetsProvider _assetsProvider;
		private readonly Transform _uiRoot;
		private readonly IObjectResolver _container;
		private readonly CancellationTokenSource _cts = new();

		public UIProvider(
			ILoggingService logger,
			IEventBus eventBus,
			IAssetsProvider assetsProvider,
			Transform uiRoot,
			IObjectResolver container)
		{
			_logger = logger;
			_eventBus = eventBus;
			_assetsProvider = assetsProvider;
			_uiRoot = uiRoot;
			_container = container;

			// Initialize layer dictionaries
			foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
			{
				_layerPresenters[layer] = new List<IPresenter<IView>>();
			}
		}

		public void Initialize()
		{
			InitializeLayers();
			_logger.Log("UI Service initialized");
		}

		private void InitializeLayers()
		{
			foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
			{
				var layerObject = new GameObject($"Layer_{layer}");
				layerObject.transform.SetParent(_uiRoot, false);

				var rectTransform = layerObject.AddComponent<RectTransform>();
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				rectTransform.sizeDelta = Vector2.zero;
				rectTransform.anchoredPosition = Vector2.zero;

				_layerRoots[layer] = layerObject.transform;

				// Set sorting order based on layer enum value
				var canvas = layerObject.AddComponent<Canvas>();
				canvas.overrideSorting = true;
				canvas.sortingOrder = (int)layer;

				// Add a raycaster to each layer
				layerObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
			}
		}

		public async UniTask<TPresenter> ShowWindow<TPresenter>(string viewAddress, UILayer layer = UILayer.Content, CancellationToken cancellationToken = default)
			where TPresenter : IPresenter<IView>
		{
			return await ShowWindowInternal<TPresenter>(viewAddress, layer, cancellationToken);
		}

		public async UniTask<TPresenter> ShowWindow<TPresenter>(AssetReference viewReference, UILayer layer = UILayer.Content, CancellationToken cancellationToken = default)
			where TPresenter : IPresenter<IView>
		{
			return await ShowWindowInternal<TPresenter>(viewReference, layer, cancellationToken);
		}

		private async UniTask<TPresenter> ShowWindowInternal<TPresenter>(object viewRefOrAddress, UILayer layer, CancellationToken cancellationToken)
			where TPresenter : IPresenter<IView>
		{
			try
			{
				var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

				// Return existing presenter if it's already active
				if (_activePresenters.TryGetValue(typeof(TPresenter), out var existingPresenter))
				{
					await existingPresenter.Show(linkedCts.Token);
					return (TPresenter)existingPresenter;
				}

				// Load view prefab
				GameObject viewPrefab = await LoadViewPrefabAsync(viewRefOrAddress);
				if (viewPrefab == null)
				{
					return default;
				}

				// Create view instance
				IView view = await CreateViewInstanceAsync(viewPrefab, layer);
				if (view == null)
				{
					_assetsProvider.Release(viewPrefab);
					return default;
				}

				// Create presenter
				TPresenter presenter = await CreatePresenterAsync<TPresenter>(view);
				if (presenter == null)
				{
					_assetsProvider.Release(viewPrefab);
					DestroyViewInstance(view);
					return default;
				}

				// Register presenter
				RegisterPresenter(presenter, layer);

				// Initialize and show
				presenter.Initialize();
				await presenter.Show(linkedCts.Token);

				return presenter;
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to show presenter {typeof(TPresenter)}");
				return default;
			}
		}

		private async UniTask<GameObject> LoadViewPrefabAsync(object viewRefOrAddress)
		{
			try
			{
				GameObject viewPrefab;

				if (viewRefOrAddress is AssetReference assetRef)
				{
					viewPrefab = await _assetsProvider.LoadAssetAsync<GameObject>(assetRef);
				}
				else if (viewRefOrAddress is string address)
				{
					viewPrefab = await _assetsProvider.LoadAssetAsync<GameObject>(address);
				}
				else
				{
					_logger.LogError($"Invalid view reference type");
					return null;
				}

				if (viewPrefab == null)
				{
					_logger.LogError("Failed to load view prefab");
				}

				return viewPrefab;
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Failed to load view prefab");
				return null;
			}
		}

		private async UniTask<IView> CreateViewInstanceAsync(GameObject viewPrefab, UILayer layer)
		{
			try
			{
				// Get the appropriate layer transform
				Transform layerTransform = _layerRoots.TryGetValue(layer, out var rootTransform) ?
					rootTransform : _uiRoot;

				GameObject viewInstance = UnityEngine.Object.Instantiate(viewPrefab, layerTransform);
				IView view = viewInstance.GetComponent<IView>();

				if (view == null)
				{
					_logger.LogError("View prefab does not have a component inheriting from BaseView");
					UnityEngine.Object.Destroy(viewInstance);
					return null;
				}

				// Set the layer for the view
				view.Layer = layer;

				return view;
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Failed to create view instance");
				return null;
			}
		}

		private async UniTask<TPresenter> CreatePresenterAsync<TPresenter>(IView view) where TPresenter : IPresenter<IView>
		{
			try
			{
				// Create a temporary scope with required dependencies
				var scope = _container.CreateScope(builder =>
				{
					// Get presenter's required view type from its constructor
					Type presenterType = typeof(TPresenter);
					var constructors = presenterType.GetConstructors();

					if (constructors.Length == 0)
					{
						_logger.LogError($"No public constructors found for presenter type {presenterType}");
						return;
					}

					// Get the first constructor's parameter types
					var constructorParams = constructors[0].GetParameters();

					// Find the view parameter (should be the first one that implements IView)
					var viewParam = constructorParams.FirstOrDefault(p =>
						typeof(IView).IsAssignableFrom(p.ParameterType));

					if (viewParam != null)
					{
						// Register the view specifically for the exact parameter type needed
						builder.RegisterInstance(view).As(viewParam.ParameterType);
					}
					else
					{
						// Fallback - just register as IView if no specific view interface found
						builder.RegisterInstance(view).As<IView>();
					}

					// Register the presenter type
					builder.Register<TPresenter>(Lifetime.Transient);
				});

				// Resolve the presenter from the container
				if (scope.TryResolve<TPresenter>(out var presenter))
				{
					return presenter;
				}

				// If we still couldn't resolve it, log an error
				_logger.LogError($"Failed to resolve presenter of type {typeof(TPresenter)} from container. " +
					$"Make sure it has appropriate constructor parameters that VContainer can resolve.");
				return default;
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to create presenter of type {typeof(TPresenter)}");
				return default;
			}
		}

		private void RegisterPresenter(IPresenter<IView> presenter, UILayer layer)
		{
			// Add to active presenters dictionary
			_activePresenters[presenter.GetType()] = presenter;

			// Add to layer tracking
			if (!_layerPresenters.ContainsKey(layer))
			{
				_layerPresenters[layer] = new List<IPresenter<IView>>();
			}
			_layerPresenters[layer].Add(presenter);
		}

		private void DestroyViewInstance(IView view)
		{
			if (view != null && view.GameObject != null)
			{
				UnityEngine.Object.Destroy(view.GameObject);
			}
		}

		public async UniTask HideWindow<TPresenter>(CancellationToken cancellationToken = default)
			where TPresenter : IPresenter<IView>
		{
			if (!_activePresenters.TryGetValue(typeof(TPresenter), out var presenter))
				return;

			var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

			try
			{
				await presenter.Hide(linkedCts.Token);
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to hide presenter {typeof(TPresenter)}");
			}
		}

		public async UniTask HideLayer(UILayer layer, CancellationToken cancellationToken = default)
		{
			if (!_layerPresenters.TryGetValue(layer, out var presenters) || presenters.Count == 0)
				return;

			var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

			try
			{
				// Create a copy to avoid collection modified exceptions
				var presentersToHide = presenters.ToArray();
				foreach (var presenter in presentersToHide)
				{
					await presenter.Hide(linkedCts.Token);
				}
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to hide layer {layer}");
			}
		}

		public async UniTask ReleaseWindow<TPresenter>(CancellationToken cancellationToken = default)
			where TPresenter : IPresenter<IView>
		{
			if (!_activePresenters.TryGetValue(typeof(TPresenter), out var presenter))
				return;

			try
			{
				// Find which layer this presenter belongs to
				UILayer? presenterLayer = FindPresenterLayer(presenter);

				// Remove from layer tracking if found
				if (presenterLayer.HasValue)
				{
					_layerPresenters[presenterLayer.Value].Remove(presenter);
				}

				// Remove from active presenters
				_activePresenters.Remove(typeof(TPresenter));

				// Dispose presenter - the presenter is responsible for cleaning up its view
				presenter.Dispose();
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to release window {typeof(TPresenter)}");
			}
		}

		private UILayer? FindPresenterLayer(IPresenter<IView> presenter)
		{
			foreach (var layerPair in _layerPresenters)
			{
				if (layerPair.Value.Contains(presenter))
				{
					return layerPair.Key;
				}
			}
			return null;
		}

		public async UniTask ReleaseLayer(UILayer layer, CancellationToken cancellationToken = default)
		{
			if (!_layerPresenters.TryGetValue(layer, out var presenters) || presenters.Count == 0)
				return;

			try
			{
				// Create a copy to avoid collection modified exceptions during iteration
				var presentersToRelease = presenters.ToArray();

				// Release all resources
				foreach (var presenter in presentersToRelease)
				{
					// Find presenter type in active presenters and remove it
					foreach (var kvp in _activePresenters.ToList())
					{
						if (kvp.Value == presenter)
						{
							_activePresenters.Remove(kvp.Key);
							break;
						}
					}

					// Dispose presenter - the presenter is responsible for cleaning up its view
					presenter.Dispose();
				}

				// Clear the layer's presenter list
				_layerPresenters[layer].Clear();
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to release layer {layer}");
			}
		}

		public TPresenter GetWindow<TPresenter>() where TPresenter : IPresenter<IView>
		{
			return _activePresenters.TryGetValue(typeof(TPresenter), out var presenter) ? (TPresenter)presenter : default;
		}

		public bool IsWindowActive<TPresenter>() where TPresenter : IPresenter<IView>
		{
			return _activePresenters.ContainsKey(typeof(TPresenter));
		}

		public void Dispose()
		{
			_cts.Cancel();
			_cts.Dispose();

			// Dispose all active presenters
			foreach (var presenter in _activePresenters.Values)
			{
				presenter?.Dispose();
			}

			_activePresenters.Clear();

			// Clear layer caches
			foreach (var layerPresenters in _layerPresenters.Values)
			{
				layerPresenters.Clear();
			}
			_layerPresenters.Clear();
			_layerRoots.Clear();
		}
	}
}