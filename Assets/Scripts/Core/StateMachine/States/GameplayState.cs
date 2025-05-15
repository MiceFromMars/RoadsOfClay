using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.Data.SaveLoad;
using ROC.Game.Enemy;
using ROC.Game.Levels;
using ROC.Game.PlayerBeh;
using UnityEngine;
using VContainer.Unity;
using ROC.Core.Assets;
using ROC.UI;
using ROC.UI.HUD;
using ROC.UI.GameOver;
using ROC.Game.Cam;
using ROC.UI.Loading;

namespace ROC.Core.StateMachine.States
{
	public class GameplayState : IPayloadedState<int>, IDisposable
	{
		private GameStateMachine _stateMachine;
		private readonly ISaveLoadService _saveLoadService;
		private readonly IEventBus _eventBus;
		private readonly ILevelProvider _levelProvider;
		private readonly ILoggingService _logger;
		private readonly IPlayerProvider _playerProvider;
		private readonly ICameraProvider _cameraProvider;
		private readonly IEnemyFactory _enemyFactory;
		private readonly IUIProvider _uiProvider;

		private PlayerProgressData _progressData;
		private CancellationTokenSource _gameplayCts;
		private int _currentLevelIndex;
		private bool _isGameOver;
		private int _currentScore;
		private float _currentMaxHeight;
		private float _currentMaxSpeed;

		// Property for setting the state machine after construction
		public GameStateMachine StateMachine
		{
			set { _stateMachine = value; }
		}

		public GameplayState(
			ISaveLoadService saveLoadService,
			IEventBus eventBus,
			ILevelProvider levelProvider,
			ILoggingService logger,
			IPlayerProvider playerProvider,
			ICameraProvider cameraProvider,
			IEnemyFactory enemyFactory,
			IUIProvider uiProvider)
		{
			_saveLoadService = saveLoadService;
			_eventBus = eventBus;
			_levelProvider = levelProvider;
			_logger = logger;
			_playerProvider = playerProvider;
			_cameraProvider = cameraProvider;
			_enemyFactory = enemyFactory;
			_uiProvider = uiProvider;
		}

		public async UniTask Enter(int levelIndex, CancellationToken cancellationToken)
		{
			_logger.Log($"GameplayState: Entering level {levelIndex}...");

			_currentLevelIndex = levelIndex;
			_gameplayCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			_isGameOver = false;
			_currentScore = 0;
			_currentMaxHeight = 0;
			_currentMaxSpeed = 0;

			try
			{
				await LoadProgressData(_gameplayCts.Token);

				await SetupLevel(_gameplayCts.Token);

				await SetupPlayer(_gameplayCts.Token);

				await SetupCamera(_gameplayCts.Token);

				await SetupHUD(_gameplayCts.Token);

				await InitializeUIData(_gameplayCts.Token);

				await SetupEnemiesSpawning(_gameplayCts.Token);

				SubscribeToEvents();

				await _uiProvider.HideLayer(UILayer.Loading, _gameplayCts.Token);

				_logger.Log("GameplayState: Level setup complete");
			}
			catch (OperationCanceledException)
			{
				// Setup was intentionally cancelled, no need to log
				_logger.Log("GameplayState: Setup was cancelled");
			}
			catch (Exception ex)
			{
				_logger.LogException(ex, "Unexpected error during gameplay initialization");

				await HandleSetupFailure();
			}
		}

		private void SubscribeToEvents()
		{
			_eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
			_eventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
			_eventBus.Subscribe<MaxHeightChangedEvent>(OnMaxHeightChanged);
			_eventBus.Subscribe<MaxSpeedChangedEvent>(OnMaxSpeedChanged);
		}

		private void UnsubscribeFromEvents()
		{
			_eventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
			_eventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
			_eventBus.Unsubscribe<MaxHeightChangedEvent>(OnMaxHeightChanged);
			_eventBus.Unsubscribe<MaxSpeedChangedEvent>(OnMaxSpeedChanged);
		}

		private void OnPlayerDied(PlayerDiedEvent eventData)
		{
			if (_isGameOver)
				return;

			_isGameOver = true;

			// Update progress data
			UpdateProgressData(eventData.FinalScore, eventData.MaxHeight, eventData.MaxSpeed);

			// Show game over screen
			_uiProvider.ShowWindow<GameOverPresenter>(AssetsKeys.GameOverView, UILayer.Content, _gameplayCts.Token)
				.ContinueWith(screen =>
				{
					screen.SetGameOverData(false, _currentLevelIndex, _currentScore);
				})
				.Forget();

			// Save progress
			try
			{
				SaveProgressAndShowGameOver(false).Forget();
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error saving progress: {ex.Message}");
				ReturnToMainMenu().Forget();
			}
		}

		private async UniTask SaveProgressAndShowGameOver(bool isWin)
		{
			try
			{
				// Save progress data
				await _saveLoadService.SaveProgress(_progressData, _gameplayCts.Token);

				// Subscribe to relevant Game Over UI events
				_eventBus.Subscribe<ReturnToMainMenuEvent>(OnReturnToMainMenu);
				_eventBus.Subscribe<RestartLevelEvent>(OnRestartLevel);
				_eventBus.Subscribe<NextLevelEvent>(OnNextLevel);
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error in SaveProgressAndShowGameOver: {ex.Message}");
				await ReturnToMainMenu();
			}
		}

		private async UniTask HandleSetupFailure()
		{
			_gameplayCts.Cancel();
			await ReturnToMainMenu();
		}

		private async UniTask ReturnToMainMenu()
		{
			UnsubscribeFromEvents();
			await _stateMachine.Enter<MainMenuState>();
		}

		private void OnEnemyKilled(EnemyKilledEvent eventData)
		{
			_currentScore += eventData.Points;
			_eventBus.Fire(new UI.ScoreChangedEvent { Score = _currentScore });
		}

		private void OnMaxHeightChanged(MaxHeightChangedEvent eventData)
		{
			_currentMaxHeight = eventData.NewMaxHeight;
			_eventBus.Fire(new UI.HeightChangedEvent { Height = _currentMaxHeight });
		}

		private void OnMaxSpeedChanged(MaxSpeedChangedEvent eventData)
		{
			_currentMaxSpeed = eventData.NewMaxSpeed;
			_eventBus.Fire(new UI.SpeedChangedEvent { Speed = _currentMaxSpeed });
		}

		private void UpdateProgressData(int finalScore, float maxHeight, float maxSpeed)
		{
			// Update total score
			_progressData.TotalScore += finalScore;

			// Update global max stats
			if (maxHeight > _progressData.MaxHeight)
				_progressData.MaxHeight = maxHeight;

			if (maxSpeed > _progressData.MaxSpeed)
				_progressData.MaxSpeed = maxSpeed;

			// Find or create level progress
			LevelProgressData levelProgress = _progressData.LevelProgress.Find(lp => lp.LevelIndex == _currentLevelIndex);

			if (levelProgress == null)
			{
				levelProgress = new LevelProgressData
				{
					LevelIndex = _currentLevelIndex,
					IsUnlocked = true,
					MaxScore = finalScore,
					MaxHeight = maxHeight,
					MaxSpeed = maxSpeed
				};
				_progressData.LevelProgress.Add(levelProgress);
			}
			else
			{
				// Update level specific stats
				if (finalScore > levelProgress.MaxScore)
					levelProgress.MaxScore = finalScore;

				if (maxHeight > levelProgress.MaxHeight)
					levelProgress.MaxHeight = maxHeight;

				if (maxSpeed > levelProgress.MaxSpeed)
					levelProgress.MaxSpeed = maxSpeed;
			}
		}

		private void CompleteLevel()
		{
			if (_isGameOver)
				return;

			_isGameOver = true;

			// Update progress data with level completion
			UpdateProgressDataWithLevelCompletion();

			// Show game over screen with win state
			_uiProvider.ShowWindow<GameOverPresenter>(AssetsKeys.GameOverView, UILayer.Content, _gameplayCts.Token)
				.ContinueWith(presenter =>
				{
					presenter.SetGameOverData(true, _currentLevelIndex, _currentScore);
				})
				.Forget();

			// Save progress
			try
			{
				SaveProgressAndShowGameOver(true).Forget();
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error saving progress on level completion: {ex.Message}");
				ReturnToMainMenu().Forget();
			}
		}

		private void OnReturnToMainMenu(ReturnToMainMenuEvent evt)
		{
			ReturnToMainMenu().Forget();
		}

		private void OnRestartLevel(RestartLevelEvent evt)
		{
			_stateMachine.Enter<GameplayState, int>(evt.LevelIndex).Forget();
		}

		private async void OnNextLevel(NextLevelEvent evt)
		{
			await _uiProvider.ShowWindow<LoadingPresenter>(AssetsKeys.Loading, UILayer.Loading);

			_stateMachine.Enter<GameplayState, int>(evt.LevelIndex).Forget();
		}

		public async UniTask Enter(CancellationToken cancellationToken)
		{
			// This method is used by non-payloaded state interface
			// In this case, we'll default to level 0
			await Enter(0, cancellationToken);
		}

		public async UniTask Exit(CancellationToken cancellationToken)
		{
			UnsubscribeFromEvents();

			await _uiProvider.HideLayer(UILayer.Content, cancellationToken);

			if (_playerProvider.CurrentPlayer != null)
			{
				_playerProvider.CurrentPlayer.gameObject.SetActive(false);
			}

			if (_cameraProvider.CurrentCamera != null)
			{
				_cameraProvider.CurrentCamera.gameObject.SetActive(false);
			}

			await _levelProvider.UnloadLevel(cancellationToken);

			_gameplayCts?.Cancel();
			_gameplayCts?.Dispose();
			_gameplayCts = null;
		}

		private void UpdateProgressDataWithLevelCompletion()
		{
			// Get or create the level progress record
			var levelProgress = _progressData.LevelProgress.FirstOrDefault(lp => lp.LevelIndex == _currentLevelIndex);
			if (levelProgress == null)
			{
				levelProgress = new LevelProgressData
				{
					LevelIndex = _currentLevelIndex,
					IsUnlocked = true
				};
				_progressData.LevelProgress.Add(levelProgress);
			}

			// LevelProgressData doesn't have an IsCompleted property, so we just ensure it's unlocked
			levelProgress.IsUnlocked = true;

			// Update the best score if current score is higher
			if (_currentScore > levelProgress.MaxScore)
			{
				levelProgress.MaxScore = _currentScore;
			}

			// Unlock next level
			var nextLevelProgress = _progressData.LevelProgress.FirstOrDefault(lp => lp.LevelIndex == _currentLevelIndex + 1);
			if (nextLevelProgress == null)
			{
				nextLevelProgress = new LevelProgressData
				{
					LevelIndex = _currentLevelIndex + 1,
					IsUnlocked = true,
					MaxScore = 0,
					MaxHeight = 0,
					MaxSpeed = 0
				};
				_progressData.LevelProgress.Add(nextLevelProgress);
			}
			else
			{
				nextLevelProgress.IsUnlocked = true;
			}
		}

		public void Dispose()
		{
			// Ensure we clean up the cancellation token source
			_gameplayCts?.Dispose();
		}

		#region Player Creation
		private async UniTask<Player> CreatePlayerAtSpawnPoint(CancellationToken cancellationToken)
		{
			_logger.Log("GameplayState: Creating player...");
			Vector3 spawnPosition = _levelProvider.CurrentLevel.GetPlayerSpawnPoint();
			return await _playerProvider.CreatePlayer(spawnPosition, cancellationToken);
		}

		private void ResetPlayerAtSpawnPoint()
		{
			Vector3 spawnPosition = _levelProvider.CurrentLevel.GetPlayerSpawnPoint();
			_playerProvider.CurrentPlayer.ResetPlayer(spawnPosition);
		}

		private async UniTask SetupPlayer(CancellationToken cancellationToken)
		{
			// Create player
			if (_playerProvider.CurrentPlayer == null)
			{
				await CreatePlayerAtSpawnPoint(cancellationToken);
			}
			else
			{
				ResetPlayerAtSpawnPoint();
			}

			if (_playerProvider.CurrentPlayer == null)
			{
				_logger.LogError("Player is null after creation attempt");
				await HandleSetupFailure();
				throw new OperationCanceledException(cancellationToken);
			}
		}
		#endregion

		#region Camera Creation
		private async UniTask SetupCamera(CancellationToken cancellationToken)
		{
			// Create camera
			if (_cameraProvider.CurrentCamera == null)
			{
				_logger.Log("GameplayState: Creating camera...");
				await _cameraProvider.CreateCamera(cancellationToken);
			}

			if (_cameraProvider.CurrentCamera != null && _playerProvider.CurrentPlayer != null)
			{
				_cameraProvider.SetTarget(_playerProvider.CurrentPlayer.transform);
			}
			else if (_cameraProvider.CurrentCamera == null)
			{
				_logger.LogError("Camera is null after creation attempt");
				await HandleSetupFailure();
				throw new OperationCanceledException(cancellationToken);
			}
		}
		#endregion

		#region Setup
		private async UniTask LoadProgressData(CancellationToken cancellationToken)
		{
			_logger.Log("GameplayState: Loading progress data...");
			_progressData = await _saveLoadService.LoadProgress(cancellationToken);
		}

		private async UniTask SetupLevel(CancellationToken cancellationToken)
		{
			_logger.Log($"GameplayState: Loading level {_currentLevelIndex}...");
			await _levelProvider.LoadLevel(_currentLevelIndex, cancellationToken);

			if (_levelProvider.CurrentLevel == null)
			{
				_logger.LogError("Level is null after loading attempt");
				await HandleSetupFailure();
				throw new OperationCanceledException(cancellationToken);
			}

			if (_levelProvider.CurrentLevelConfig == null)
			{
				_logger.LogError("Level config is null after loading attempt");
				await HandleSetupFailure();
				throw new OperationCanceledException(cancellationToken);
			}
		}

		private async UniTask SetupHUD(CancellationToken cancellationToken)
		{
			_logger.Log("GameplayState: Showing game HUD...");
			var hudPresenter = await _uiProvider.ShowWindow<GameHUDPresenter>(AssetsKeys.GameplayHUDView, UILayer.Content, cancellationToken);

			if (hudPresenter == null)
			{
				_logger.LogError("HUD presenter is null after creation attempt");
				await HandleSetupFailure();
				throw new OperationCanceledException(cancellationToken);
			}
		}

		private async UniTask InitializeUIData(CancellationToken cancellationToken)
		{
			var config = await _playerProvider.GetPlayerConfig(cancellationToken);

			if (config == null)
			{
				_logger.LogError("Player config is null after retrieval attempt");
				await HandleSetupFailure();
				throw new OperationCanceledException(cancellationToken);
			}

			// Initial UI updates
			_eventBus.Fire(new PlayerLivesChangedEvent
			{
				CurrentLives = config.StartLives,
				MaxLives = config.StartLives
			});

			_eventBus.Fire(new UI.ScoreChangedEvent { Score = 0 });
			_eventBus.Fire(new UI.HeightChangedEvent { Height = 0 });
			_eventBus.Fire(new UI.SpeedChangedEvent { Speed = 0 });
		}

		private async UniTask SetupEnemiesSpawning(CancellationToken cancellationToken)
		{
			// Initialize enemy pools
			_logger.Log("GameplayState: Initializing enemy pools...");
			var spawnConfigs = _levelProvider.CurrentLevelConfig.EnemySpawnConfigs;

			if (spawnConfigs == null)
			{
				_logger.LogError("Enemy spawn configs are null");
				await HandleSetupFailure();
				throw new OperationCanceledException(cancellationToken);
			}

			await _enemyFactory.InitializePoolsForLevel(spawnConfigs, cancellationToken);

			// Start enemy spawning
			_logger.Log("GameplayState: Starting enemy spawning...");
			await _enemyFactory.StartSpawning(spawnConfigs, cancellationToken);
		}
		#endregion

	}
}
