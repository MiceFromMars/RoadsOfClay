using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.Data.Config;
using ROC.Data.SaveLoad;
using ROC.UI;
using ROC.UI.MainMenu;
using ROC.UI.MainMenu.LevelSelection;
using VContainer;
using VContainer.Unity;

namespace ROC.Core.StateMachine.States
{
	public class MainMenuState : IState
	{
		private readonly GameStateMachine _stateMachine;
		private readonly ISaveLoadService _saveLoadService;
		private readonly IEventBus _eventBus;
		private readonly IObjectResolver _container;
		private readonly IUIService _uiService;
		private CancellationTokenSource _cts;
		private LevelSelectionScreen _levelSelectionScreen;

		public MainMenuState(
			GameStateMachine stateMachine,
			ISaveLoadService saveLoadService,
			IEventBus eventBus,
			IObjectResolver container,
			IUIService uiService)
		{
			_stateMachine = stateMachine;
			_saveLoadService = saveLoadService;
			_eventBus = eventBus;
			_container = container;
			_uiService = uiService;
		}

		public async UniTask Enter(CancellationToken cancellationToken)
		{
			// Create internal cancellation token source
			_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			// Load player progress data
			PlayerProgressData progressData = await _saveLoadService.LoadProgress(_cts.Token);

			// Show main menu UI
			await _uiService.Show<MainMenuScreen>("UI/MainMenuScreen", _cts.Token);

			// Subscribe to UI events
			_eventBus.Subscribe<LevelSelectedEvent>(OnLevelSelected);
			_eventBus.Subscribe<LevelSelectionOpenedEvent>(OnLevelSelectionOpened);
			_eventBus.Subscribe<MainMenuOpenedEvent>(OnReturnToMainMenu);
		}

		public async UniTask Exit(CancellationToken cancellationToken)
		{
			// Unsubscribe from events
			_eventBus.Unsubscribe<LevelSelectedEvent>(OnLevelSelected);
			_eventBus.Unsubscribe<LevelSelectionOpenedEvent>(OnLevelSelectionOpened);
			_eventBus.Unsubscribe<MainMenuOpenedEvent>(OnReturnToMainMenu);

			// Hide all UI screens
			await _uiService.Hide<MainMenuScreen>(cancellationToken);

			if (_levelSelectionScreen != null)
			{
				await _uiService.Hide<LevelSelectionScreen>(cancellationToken);
			}

			// Dispose the internal cancellation token source
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = null;
		}

		private void OnLevelSelected(LevelSelectedEvent evt)
		{
			_stateMachine.Enter<GameplayState, int>(evt.LevelIndex).Forget();
		}

		private async void OnLevelSelectionOpened(LevelSelectionOpenedEvent evt)
		{
			// Hide main menu before showing level selection
			await _uiService.Hide<MainMenuScreen>(_cts.Token);

			// Show level selection UI
			_levelSelectionScreen = await _uiService.Show<LevelSelectionScreen>("UI/LevelSelectionScreen", _cts.Token);
		}

		private async void OnReturnToMainMenu(MainMenuOpenedEvent evt)
		{
			// Hide level selection before showing main menu
			if (_levelSelectionScreen != null)
			{
				await _uiService.Hide<LevelSelectionScreen>(_cts.Token);
				_levelSelectionScreen = null;
			}

			// Show main menu
			await _uiService.Show<MainMenuScreen>("UI/MainMenuScreen", _cts.Token);
		}
	}
}
