using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.Data.Config;
using ROC.Data.SaveLoad;
using ROC.UI;
using ROC.UI.Loading;
using ROC.UI.MainMenu;
using ROC.UI.MainMenu.LevelSelection;
using VContainer;
using VContainer.Unity;

namespace ROC.Core.StateMachine.States
{
	public class MainMenuState : IState
	{
		// Changed from readonly to allow property injection
		private GameStateMachine _stateMachine;
		private readonly ISaveLoadService _saveLoadService;
		private readonly IEventBus _eventBus;
		private readonly IObjectResolver _container;
		private readonly IUIProvider _uiProvider;
		private readonly ILoggingService _logger;
		private CancellationTokenSource _cts;
		private LevelSelectionPresenter _levelSelectionPresenter;

		// Property for setting the state machine after construction
		public GameStateMachine StateMachine
		{
			set { _stateMachine = value; }
		}

		public MainMenuState(
			ISaveLoadService saveLoadService,
			IEventBus eventBus,
			IObjectResolver container,
			IUIProvider uiProvider,
			ILoggingService logger)
		{
			_saveLoadService = saveLoadService;
			_eventBus = eventBus;
			_container = container;
			_uiProvider = uiProvider;
			_logger = logger;
		}

		public async UniTask Enter(CancellationToken cancellationToken)
		{
			_logger.Log("MainMenuState: Entering...");

			// Create internal cancellation token source
			_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			// Show main menu UI
			_logger.Log("MainMenuState: Showing main menu UI...");
			await _uiProvider.ShowWindow<MainMenuPresenter>(AssetsKeys.MainMenuView, UILayer.Content, _cts.Token);
			await _uiProvider.HideLayer(UILayer.Loading, _cts.Token);

			// Subscribe to UI events
			_logger.Log("MainMenuState: Subscribing to UI events...");
			_eventBus.Subscribe<LevelSelectedEvent>(OnLevelSelected);
			_eventBus.Subscribe<LevelSelectionOpenedEvent>(OnLevelSelectionOpened);
			_eventBus.Subscribe<MainMenuOpenedEvent>(OnReturnToMainMenu);

			_logger.Log("MainMenuState: Entered successfully");
		}

		public async UniTask Exit(CancellationToken cancellationToken)
		{
			_logger.Log("MainMenuState: Exiting...");

			// Unsubscribe from events
			_eventBus.Unsubscribe<LevelSelectedEvent>(OnLevelSelected);
			_eventBus.Unsubscribe<LevelSelectionOpenedEvent>(OnLevelSelectionOpened);
			_eventBus.Unsubscribe<MainMenuOpenedEvent>(OnReturnToMainMenu);

			// Hide all UI screens
			await _uiProvider.HideWindow<MainMenuPresenter>(cancellationToken);

			if (_levelSelectionPresenter != null)
			{
				await _uiProvider.HideWindow<LevelSelectionPresenter>(cancellationToken);
			}

			// Dispose the internal cancellation token source
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = null;

			_logger.Log("MainMenuState: Exited successfully");
		}

		private async void OnLevelSelected(LevelSelectedEvent evt)
		{
			await _uiProvider.HideLayer(UILayer.Content, _cts.Token);

			await _uiProvider.ShowWindow<LoadingPresenter>(AssetsKeys.LoadingView, UILayer.Loading, _cts.Token);

			_logger.Log($"MainMenuState: Level {evt.LevelIndex} selected, transitioning to GameplayState...");
			_stateMachine.Enter<GameplayState, int>(evt.LevelIndex).Forget();
		}

		private async void OnLevelSelectionOpened(LevelSelectionOpenedEvent evt)
		{
			_logger.Log("MainMenuState: Level selection opened...");
			// Hide main menu before showing level selection
			await _uiProvider.HideWindow<MainMenuPresenter>(_cts.Token);

			// Show level selection UI
			_levelSelectionPresenter = await _uiProvider.ShowWindow<LevelSelectionPresenter>(AssetsKeys.LevelSelectionView, UILayer.Content, _cts.Token);
		}

		private async void OnReturnToMainMenu(MainMenuOpenedEvent evt)
		{
			_logger.Log("MainMenuState: Returning to main menu...");
			// Hide level selection before showing main menu
			if (_levelSelectionPresenter != null)
			{
				await _uiProvider.HideWindow<LevelSelectionPresenter>(_cts.Token);
				_levelSelectionPresenter = null;
			}

			// Show main menu
			await _uiProvider.ShowWindow<MainMenuPresenter>("UI/MainMenuScreen", UILayer.Content, _cts.Token);
		}
	}
}
