using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Data.SaveLoad;
using ROC.UI;
using ROC.UI.Loading;

namespace ROC.Core.StateMachine.States
{
	public class BootstrapState : IState, IDisposable
	{
		private GameStateMachine _stateMachine;
		private readonly ISaveLoadService _saveLoadService;
		private readonly ILoggingService _logger;
		private readonly IAssetsProvider _assetsProvider;
		private readonly IUIProvider _uiProvider;

		public GameStateMachine StateMachine
		{
			set { _stateMachine = value; }
		}

		public BootstrapState(
			IAssetsProvider assetsProvider,
			ISaveLoadService saveLoadService,
			ILoggingService logger,
			IUIProvider uiProvider)
		{
			_saveLoadService = saveLoadService;
			_logger = logger;
			_assetsProvider = assetsProvider;
			_uiProvider = uiProvider;
		}

		public async UniTask Enter(CancellationToken cancellationToken)
		{
			try
			{
				await _assetsProvider.InitializeAsync(cancellationToken);

				// Show loading screen
				await _uiProvider.ShowWindow<LoadingPresenter>(AssetsKeys.LoadingView, UILayer.Loading, cancellationToken);

				await _saveLoadService.LoadProgress(cancellationToken);

				await _stateMachine.Enter<MainMenuState>();
			}
			catch (Exception ex)
			{
				_logger.LogException(ex, "Bootstrap state entering");
				// In a real application, you might want to handle critical errors here
				// e.g., show an error screen, try to recover, etc.
			}
		}

		public async UniTask Exit(CancellationToken cancellationToken)
		{
			await UniTask.CompletedTask;
		}

		public void Dispose()
		{

		}
	}
}
