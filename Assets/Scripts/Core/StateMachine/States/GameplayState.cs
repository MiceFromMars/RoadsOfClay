using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.Data.Config;
using ROC.Data.SaveLoad;
using ROC.Game.Enemy;
using ROC.Game.Levels;
using ROC.Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using UnityEngine.ResourceManagement.ResourceProviders;
using ROC.Core.Assets;
using ROC.UI;
using ROC.UI.HUD;

namespace ROC.Core.StateMachine.States
{
	public class GameplayState : IPayloadedState<int>, IDisposable
	{
		private readonly GameStateMachine _stateMachine;
		private readonly ISaveLoadService _saveLoadService;
		private readonly IEventBus _eventBus;
		private readonly IObjectResolver _container;
		private readonly ILevelProvider _levelProvider;
		private readonly IAssetsProvider _assetsProvider;
		private readonly ILoggingService _logger;
		private readonly IPlayerProvider _playerProvider;
		private readonly ICameraProvider _cameraProvider;
		private readonly IEnemyFactory _enemyFactory;
		private readonly IUIService _uiService;

		private PlayerBehavior _player;
		private Camera _camera;
		private PlayerInputHandler _inputHandler;
		private PlayerProgressData _progressData;
		private CancellationTokenSource _gameplayCts;
		private SceneInstance _gameplaySceneInstance;
		private int _currentLevelIndex;
		private bool _isGameOver;
		private int _currentScore;
		private float _currentMaxHeight;
		private float _currentMaxSpeed;
		private GameHUD _gameHUD;

		public GameplayState(
			GameStateMachine stateMachine,
			ISaveLoadService saveLoadService,
			IEventBus eventBus,
			IObjectResolver container,
			ILevelProvider levelProvider,
			IAssetsProvider assetsProvider,
			ILoggingService logger,
			IPlayerProvider playerProvider,
			ICameraProvider cameraProvider,
			IEnemyFactory enemyFactory,
			IUIService uiService)
		{
			_stateMachine = stateMachine;
			_saveLoadService = saveLoadService;
			_eventBus = eventBus;
			_container = container;
			_levelProvider = levelProvider;
			_assetsProvider = assetsProvider;
			_logger = logger;
			_playerProvider = playerProvider;
			_cameraProvider = cameraProvider;
			_enemyFactory = enemyFactory;
			_uiService = uiService;
		}

		public async UniTask Enter(int levelIndex, CancellationToken cancellationToken)
		{
			_currentLevelIndex = levelIndex;
			_gameplayCts = new CancellationTokenSource();
			_isGameOver = false;
			_currentScore = 0;
			_currentMaxHeight = 0;
			_currentMaxSpeed = 0;

			try
			{
				// Load progress data
				_progressData = await _saveLoadService.LoadProgress(cancellationToken);

				// Load gameplay scene
				await LoadGameplayScene(_gameplayCts.Token);

				// Setup level
				await _levelProvider.LoadLevel(_currentLevelIndex, _gameplayCts.Token);

				// Show game HUD
				_gameHUD = await _uiService.Show<GameHUD>("UI/GameHUD", _gameplayCts.Token);

				// Initial UI updates
				_eventBus.Fire(new PlayerLivesChangedEvent
				{
					CurrentLives = 3, // Default value, replace with actual player lives from config
					MaxLives = 3      // Default value, replace with actual max lives from config
				});

				_eventBus.Fire(new UI.ScoreChangedEvent { Score = 0 });
				_eventBus.Fire(new UI.HeightChangedEvent { Height = 0 });
				_eventBus.Fire(new UI.SpeedChangedEvent { Speed = 0 });

				// Create player
				Vector3 spawnPosition = _levelProvider.CurrentLevel.GetPlayerSpawnPoint();
				_player = await _playerProvider.CreatePlayer(spawnPosition, _gameplayCts.Token);

				if (_player == null)
				{
					_logger.LogError("Failed to create player");
					await ReturnToMainMenu();
					return;
				}

				// Setup input handler with GameHUD
				_inputHandler = UnityEngine.Object.FindObjectOfType<PlayerInputHandler>();
				if (_inputHandler != null)
				{
					_inputHandler.Initialize(_player);
				}

				// Create camera
				_camera = await _cameraProvider.CreateCamera(_player.transform, _gameplayCts.Token);

				if (_camera == null)
				{
					_logger.LogError("Failed to create camera");
					await ReturnToMainMenu();
					return;
				}

				// Initialize enemy pools
				var spawnConfigs = _levelProvider.CurrentLevelConfig.EnemySpawnConfigs;
				await _enemyFactory.InitializePoolsForLevel(spawnConfigs, _gameplayCts.Token);

				// Start enemy spawning
				await _enemyFactory.StartSpawning(spawnConfigs, _gameplayCts.Token);

				// Subscribe to events
				SubscribeToEvents();
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error during gameplay initialization: {ex.Message}");
				await ReturnToMainMenu();
			}
		}

		private async UniTask LoadGameplayScene(CancellationToken cancellationToken)
		{
			// Load scene through AssetsProvider
			var sceneInstance = await _assetsProvider.LoadSceneAsync(AssetsKeys.Gameplay, LoadSceneMode.Single, cancellationToken);

			if (!sceneInstance.Scene.IsValid())
			{
				throw new Exception($"Failed to load gameplay scene: {AssetsKeys.Gameplay}");
			}

			_gameplaySceneInstance = sceneInstance;
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
			_uiService.Show<GameOverScreen>("UI/GameOverScreen", _gameplayCts.Token)
				.ContinueWith(screen =>
				{
					screen.Initialize(false, _currentLevelIndex, _currentScore);
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

		private async UniTask ReturnToMainMenu()
		{
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
			_uiService.Show<GameOverScreen>("UI/GameOverScreen", _gameplayCts.Token)
				.ContinueWith(screen =>
				{
					screen.Initialize(true, _currentLevelIndex, _currentScore);
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

		private void OnNextLevel(NextLevelEvent evt)
		{
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
			await CleanupResources(cancellationToken);
			_gameplayCts?.Cancel();
			_gameplayCts?.Dispose();
			_gameplayCts = null;
		}

		private async UniTask CleanupResources(CancellationToken cancellationToken)
		{
			// Unsubscribe from events
			UnsubscribeFromEvents();

			// Note: Disconnect UI button handlers from input handler here
			// Implement specific disconnections based on the actual methods available

			// Hide UI
			await _uiService.Hide<GameHUD>(cancellationToken);
			await _uiService.Hide<GameOverScreen>(cancellationToken);

			// Clean up player
			if (_player != null)
			{
				await _playerProvider.DestroyPlayer(cancellationToken);
				_player = null;
			}

			// Clean up camera
			if (_camera != null)
			{
				await _cameraProvider.DestroyCamera(cancellationToken);
				_camera = null;
			}

			// Clean up input handler reference
			_inputHandler = null;

			// Clean up level
			await _levelProvider.UnloadLevel(cancellationToken);

			// Unload gameplay scene
			if (_gameplaySceneInstance.Scene.IsValid())
			{
				await _assetsProvider.UnloadSceneAsync(_gameplaySceneInstance, cancellationToken);
			}
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
	}
}
