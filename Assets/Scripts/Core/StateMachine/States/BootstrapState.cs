using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.Data.SaveLoad;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ROC.Core.StateMachine.States
{
	public class BootstrapState : IState, IDisposable
	{
		private readonly GameStateMachine _stateMachine;
		private readonly ISaveLoadService _saveLoadService;
		private readonly IEventBus _eventBus;
		private readonly ILoggingService _logger;

		private AsyncOperationHandle<SceneInstance> _mainMenuSceneHandle;

		public BootstrapState(
			GameStateMachine stateMachine,
			ISaveLoadService saveLoadService,
			IEventBus eventBus,
			ILoggingService logger)
		{
			_stateMachine = stateMachine;
			_saveLoadService = saveLoadService;
			_eventBus = eventBus;
			_logger = logger;
		}

		public async UniTask Enter(CancellationToken cancellationToken)
		{
			try
			{
				// Initialize Addressables
				AsyncOperationHandle<IResourceLocator> initHandle = Addressables.InitializeAsync();
				await initHandle.ToUniTask(cancellationToken: cancellationToken);

				// Load player progress
				await _saveLoadService.LoadProgress(cancellationToken);

				// Load main menu scene
				await LoadMainMenuScene(cancellationToken);

				// Transition to main menu state
				await _stateMachine.Enter<MainMenuState>();
			}
			catch (Exception ex)
			{
				_logger.LogException(ex, "Bootstrap state entering");
				// In a real application, you might want to handle critical errors here
				// e.g., show an error screen, try to recover, etc.
			}
		}

		private async UniTask LoadMainMenuScene(CancellationToken cancellationToken)
		{
			// Release any previously loaded scene
			if (_mainMenuSceneHandle.IsValid())
			{
				Addressables.Release(_mainMenuSceneHandle);
				_mainMenuSceneHandle = default;
			}

			_mainMenuSceneHandle = Addressables.LoadSceneAsync(AssetsKeys.MainMenu, LoadSceneMode.Single);
			await _mainMenuSceneHandle.ToUniTask(cancellationToken: cancellationToken);

			if (_mainMenuSceneHandle.Status != AsyncOperationStatus.Succeeded)
			{
				throw new ApplicationException($"Failed to load main menu scene: {AssetsKeys.MainMenu}");
			}
		}

		public async UniTask Exit(CancellationToken cancellationToken)
		{
			await UniTask.CompletedTask;
		}

		public void Dispose()
		{
			if (_mainMenuSceneHandle.IsValid())
				Addressables.Release(_mainMenuSceneHandle);
		}
	}
}
