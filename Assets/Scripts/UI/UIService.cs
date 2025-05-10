using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.UI.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

namespace ROC.UI
{
	public class UIService : IUIService, IInitializable
	{
		private readonly Dictionary<Type, IUIScreen> _activeScreens = new Dictionary<Type, IUIScreen>();
		private readonly ILoggingService _logger;
		private readonly IEventBus _eventBus;
		private readonly IAssetsProvider _assetsProvider;
		private readonly Transform _uiRoot;
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		public UIService(
			ILoggingService logger,
			IEventBus eventBus,
			IAssetsProvider assetsProvider,
			Transform uiRoot)
		{
			_logger = logger;
			_eventBus = eventBus;
			_assetsProvider = assetsProvider;
			_uiRoot = uiRoot;
		}

		public void Initialize()
		{
			_logger.Log("UI Service initialized");
		}

		public async UniTask<T> Show<T>(AssetReference screenReference, CancellationToken cancellationToken = default) where T : IUIScreen
		{
			try
			{
				var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

				if (_activeScreens.TryGetValue(typeof(T), out var existingScreen))
				{
					await existingScreen.Show(linkedCts.Token);
					return (T)existingScreen;
				}

				GameObject screenPrefab = await _assetsProvider.LoadAssetAsync<GameObject>(screenReference);

				if (screenPrefab == null)
				{
					_logger.LogError($"Failed to load screen prefab for {typeof(T)}");
					return default;
				}

				GameObject screenInstance = UnityEngine.Object.Instantiate(screenPrefab, _uiRoot);
				T screen = screenInstance.GetComponent<T>();

				if (screen == null)
				{
					_logger.LogError($"Screen prefab does not have component of type {typeof(T)}");
					_assetsProvider.Release(screenPrefab);
					return default;
				}

				_activeScreens[typeof(T)] = screen;
				await screen.Show(linkedCts.Token);

				return screen;
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to show screen {typeof(T)}");
				return default;
			}
		}

		public async UniTask<T> Show<T>(string screenAddress, CancellationToken cancellationToken = default) where T : IUIScreen
		{
			try
			{
				var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

				if (_activeScreens.TryGetValue(typeof(T), out var existingScreen))
				{
					await existingScreen.Show(linkedCts.Token);
					return (T)existingScreen;
				}

				GameObject screenPrefab = await _assetsProvider.LoadAssetAsync<GameObject>(screenAddress);

				if (screenPrefab == null)
				{
					_logger.LogError($"Failed to load screen prefab for {typeof(T)} at address {screenAddress}");
					return default;
				}

				GameObject screenInstance = UnityEngine.Object.Instantiate(screenPrefab, _uiRoot);
				T screen = screenInstance.GetComponent<T>();

				if (screen == null)
				{
					_logger.LogError($"Screen prefab does not have component of type {typeof(T)}");
					_assetsProvider.Release(screenPrefab);
					return default;
				}

				_activeScreens[typeof(T)] = screen;
				await screen.Show(linkedCts.Token);

				return screen;
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to show screen {typeof(T)}");
				return default;
			}
		}

		public async UniTask Hide<T>(CancellationToken cancellationToken = default) where T : IUIScreen
		{
			if (!_activeScreens.TryGetValue(typeof(T), out var screen))
				return;

			var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

			try
			{
				await screen.Hide(linkedCts.Token);
			}
			catch (Exception e)
			{
				_logger.LogException(e, $"Failed to hide screen {typeof(T)}");
			}
		}

		public T GetScreen<T>() where T : IUIScreen
		{
			return _activeScreens.TryGetValue(typeof(T), out var screen) ? (T)screen : default;
		}

		public void Dispose()
		{
			_cts.Cancel();
			_cts.Dispose();

			foreach (var screen in _activeScreens.Values)
			{
				if (screen != null)
				{
					screen.Dispose();
					if (screen is MonoBehaviour behaviour && behaviour.gameObject != null)
						UnityEngine.Object.Destroy(behaviour.gameObject);
				}
			}

			_activeScreens.Clear();
		}
	}
}