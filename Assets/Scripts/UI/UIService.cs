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
	public class UIService : IUIService, IInitializable, IDisposable
	{
		private readonly Dictionary<Type, IPresenter> _activePresenters = new();
		private readonly Dictionary<UILayer, List<IPresenter>> _layerPresenters = new();
		private readonly Dictionary<UILayer, Transform> _layerRoots = new();
		private readonly ILoggingService _logger;
		private readonly IEventBus _eventBus;
		private readonly IAssetsProvider _assetsProvider;
		private readonly Transform _uiRoot;
		private readonly IObjectResolver _container;
		private readonly CancellationTokenSource _cts = new();

		public UIService(
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
				_layerPresenters[layer] = new List<IPresenter>();
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
			where TPresenter : IPresenter
		{
			return await ShowWindowInternal<TPresenter>(viewAddress, layer, cancellationToken);
		}

		public async UniTask<TPresenter> ShowWindow<TPresenter>(AssetReference viewReference, UILayer layer = UILayer.Content, CancellationToken cancellationToken = default)
			where TPresenter : IPresenter
		{
			return await ShowWindowInternal<TPresenter>(viewReference, layer, cancellationToken);
		}

		private async UniTask<TPresenter> ShowWindowInternal<TPresenter>(object viewRefOrAddress, UILayer layer, CancellationToken cancellationToken)
			where TPresenter : IPresenter
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
					_logger.LogError("View prefab does not have a component implementing IView");
					UnityEngine.Object.Destroy(viewInstance);
					return null;
				}

				// If the view is a UIScreen, set its layer
				if (view is IUIScreen screen)
				{
					screen.Layer = layer;
				}

				return view;
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Failed to create view instance");
				return null;
			}
		}

		private async UniTask<TPresenter> CreatePresenterAsync<TPresenter>(IView view) where TPresenter : IPresenter
		{
			try
			{
				// Try to create using the DI container first by registering the view parameter
				var scope = _container.CreateScope(builder =>
				{
					builder.RegisterInstance(view).AsImplementedInterfaces().AsSelf();
				});

				// Try to resolve the presenter from the container
				if (scope.TryResolve<TPresenter>(out var diPresenter))
				{
					return diPresenter;
				}

				// Fallback to creating it using reflection if the container couldn't do it
				// Note: When using reflection, we're assuming a constructor with view and eventBus parameters
				TPresenter presenter = (TPresenter)Activator.CreateInstance(typeof(TPresenter), view, _eventBus);

				if (presenter == null)
				{
					_logger.LogError($"Failed to create presenter of type {typeof(TPresenter)}");
				}

				return presenter;
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to create presenter of type {typeof(TPresenter)}");
				return default;
			}
		}

		private void RegisterPresenter(IPresenter presenter, UILayer layer)
		{
			// Add to active presenters dictionary
			_activePresenters[presenter.GetType()] = presenter;

			// Add to layer tracking
			if (!_layerPresenters.ContainsKey(layer))
			{
				_layerPresenters[layer] = new List<IPresenter>();
			}
			_layerPresenters[layer].Add(presenter);
		}

		private void DestroyViewInstance(IView view)
		{
			if (view is MonoBehaviour viewBehaviour && viewBehaviour != null && viewBehaviour.gameObject != null)
			{
				UnityEngine.Object.Destroy(viewBehaviour.gameObject);
			}
		}

		public async UniTask HideWindow<TPresenter>(CancellationToken cancellationToken = default)
			where TPresenter : IPresenter
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
			where TPresenter : IPresenter
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

		private UILayer? FindPresenterLayer(IPresenter presenter)
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

		public TPresenter GetWindow<TPresenter>() where TPresenter : IPresenter
		{
			return _activePresenters.TryGetValue(typeof(TPresenter), out var presenter) ? (TPresenter)presenter : default;
		}

		public bool IsWindowActive<TPresenter>() where TPresenter : IPresenter
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