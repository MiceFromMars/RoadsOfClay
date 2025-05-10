using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.Data.Config;
using ROC.Game.Enemy;
using UnityEngine;
using VContainer;

namespace ROC.Game.Levels
{
	public class LevelLoader : ILevelLoader
	{
		private readonly IEventBus _eventBus;
		private readonly IAssetsProvider _assetsProvider;
		private readonly ILoggingService _logger;

		private GameObject _currentLevelInstance;
		private GameObject _levelPrefab;
		private CancellationTokenSource _levelCts;

		public Level CurrentLevel { get; private set; }
		public LevelConfig CurrentLevelConfig { get; private set; }
		public int CurrentLevelIndex { get; private set; } = -1;

		[Inject]
		public LevelLoader(
			IEventBus eventBus,
			IAssetsProvider assetsProvider,
			ILoggingService logger)
		{
			_eventBus = eventBus;
			_assetsProvider = assetsProvider;
			_logger = logger;

			_levelCts = new CancellationTokenSource();
		}

		public async UniTask<Level> LoadLevel(int levelIndex, CancellationToken cancellationToken)
		{
			if (_currentLevelInstance != null)
				await UnloadLevel(cancellationToken);

			_levelCts?.Cancel();
			_levelCts = new CancellationTokenSource();

			CurrentLevelIndex = levelIndex;

			// Load level config directly using AssetsProvider
			string levelConfigKey = $"{AssetsKeys.LevelConfig}{levelIndex}";
			CurrentLevelConfig = await _assetsProvider.LoadAssetAsync<LevelConfig>(levelConfigKey);
			if (CurrentLevelConfig == null)
			{
				_logger.LogError($"Failed to load level config for index {levelIndex}");
				return null;
			}

			// Load level prefab using AssetsKeys for consistent path formatting
			_levelPrefab = await _assetsProvider.LoadAssetAsync<GameObject>($"{AssetsKeys.LevelPrefabPath}{levelIndex}");
			if (_levelPrefab == null)
			{
				_logger.LogError($"Failed to load level: {AssetsKeys.LevelPrefabPath}{levelIndex}");
				return null;
			}

			// Instantiate level
			_currentLevelInstance = UnityEngine.Object.Instantiate(_levelPrefab);

			// Get Level component
			CurrentLevel = _currentLevelInstance.GetComponent<Level>();
			if (CurrentLevel == null)
			{
				Debug.LogWarning($"Level component not found on Level{levelIndex}, adding it dynamically");
				CurrentLevel = _currentLevelInstance.AddComponent<Level>();
			}

			return CurrentLevel;
		}

		public async UniTask UnloadLevel(CancellationToken cancellationToken)
		{
			if (_currentLevelInstance == null)
				return;

			_levelCts?.Cancel();

			// Clean up level internals
			await CleanupLevel(_levelCts.Token);

			// Clean up any spawned enemies
			await UniTask.Yield(PlayerLoopTiming.Update);

			UnityEngine.Object.Destroy(_currentLevelInstance);
			_currentLevelInstance = null;
			CurrentLevel = null;

			if (_levelPrefab != null)
			{
				_assetsProvider.Release(_levelPrefab);
				_levelPrefab = null;
			}

			// Release level config
			if (CurrentLevelConfig != null)
			{
				_assetsProvider.Release(CurrentLevelConfig);
				CurrentLevelConfig = null;
			}

			CurrentLevelIndex = -1;
		}

		public async UniTask CleanupLevel(CancellationToken cancellationToken)
		{
			_levelCts?.Cancel();

			// Additional cleanup logic can be added here
			await UniTask.CompletedTask;
		}

		public void Dispose()
		{
			_levelCts?.Cancel();
			_levelCts?.Dispose();

			if (_levelPrefab != null)
			{
				_assetsProvider.Release(_levelPrefab);
				_levelPrefab = null;
			}

			if (CurrentLevelConfig != null)
			{
				_assetsProvider.Release(CurrentLevelConfig);
				CurrentLevelConfig = null;
			}
		}
	}
}