using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.Core.Pool;
using ROC.Core.Assets;
using ROC.Data.Config;
using UnityEngine;

namespace ROC.Game.Enemy
{
	public class EnemyFactory : IEnemyFactory
	{
		private readonly IEventBus _eventBus;
		private readonly IAssetsProvider _assetsProvider;
		private readonly Dictionary<EnemyType, ObjectPool<Enemy>> _enemyPools = new();
		private readonly Dictionary<EnemyType, GameObject> _enemyPrefabs = new();
		private readonly Dictionary<EnemyType, EnemyConfig> _enemyConfigs = new();
		private readonly Dictionary<EnemyType, HashSet<Enemy>> _activeEnemies = new();
		private readonly List<CancellationTokenSource> _spawnTaskCts = new();
		private readonly Vector2 _levelSize = new Vector2(30f, 20f); // Default level size
		private bool _isDisposed;

		public EnemyFactory(IEventBus eventBus, IAssetsProvider assetsProvider)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_assetsProvider = assetsProvider ?? throw new ArgumentNullException(nameof(assetsProvider));
		}

		public async UniTask InitializePoolsForLevel(IReadOnlyList<EnemySpawnConfig> spawnConfigs, CancellationToken cancellationToken)
		{
			// Clear existing active enemies tracking for new level
			_activeEnemies.Clear();

			// Extract unique enemy types from spawn configs
			var enemyTypes = ExtractUniqueEnemyTypes(spawnConfigs);

			// Load resources and create pools for each enemy type
			foreach (var enemyType in enemyTypes)
			{
				await LoadEnemyResources(enemyType, cancellationToken);
				CreatePoolForEnemyType(enemyType, GetMaxPoolSizeForEnemyType(enemyType, spawnConfigs));

				// Initialize tracking of active enemies for this type
				if (!_activeEnemies.ContainsKey(enemyType))
				{
					_activeEnemies[enemyType] = new HashSet<Enemy>();
				}
			}
		}

		public IEnemy SpawnEnemy(EnemyType enemyType, Vector3 position)
		{
			if (!_enemyPools.TryGetValue(enemyType, out var pool))
			{
				Debug.LogError($"No pool initialized for enemy type: {enemyType}");
				return null;
			}

			Enemy enemy = pool.Get();
			enemy.SetPosition(position);

			// Track this enemy as active
			if (_activeEnemies.TryGetValue(enemyType, out var activeSet))
			{
				activeSet.Add(enemy);
			}

			return enemy;
		}

		public async UniTask StartSpawning(IReadOnlyList<EnemySpawnConfig> spawnConfigs, CancellationToken cancellationToken)
		{
			foreach (var spawnConfig in spawnConfigs)
			{
				var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				_spawnTaskCts.Add(cts);

				SpawnEnemiesForConfig(spawnConfig, cts.Token).Forget();
			}

			await UniTask.CompletedTask;
		}

		private HashSet<EnemyType> ExtractUniqueEnemyTypes(IReadOnlyList<EnemySpawnConfig> spawnConfigs)
		{
			HashSet<EnemyType> enemyTypes = new();
			foreach (var spawnConfig in spawnConfigs)
			{
				enemyTypes.Add(spawnConfig.EnemyType);
			}
			return enemyTypes;
		}

		private async UniTask SpawnEnemiesForConfig(
			EnemySpawnConfig spawnConfig,
			CancellationToken cancellationToken)
		{
			// Wait for the initial spawn delay
			await UniTask.Delay(TimeSpan.FromSeconds(spawnConfig.StartSpawnDelay), cancellationToken: cancellationToken);

			EnemyType enemyType = spawnConfig.EnemyType;

			while (!cancellationToken.IsCancellationRequested)
			{
				int currentAliveCount = GetActiveEnemyCount(enemyType);

				if (currentAliveCount < spawnConfig.MaxEnemies)
				{
					Vector3 randomPosition = GenerateRandomPosition();
					SpawnEnemy(enemyType, randomPosition);
				}

				await UniTask.Delay(TimeSpan.FromSeconds(spawnConfig.SpawnInterval), cancellationToken: cancellationToken);
			}
		}

		private int GetActiveEnemyCount(EnemyType enemyType)
		{
			return _activeEnemies.TryGetValue(enemyType, out var activeSet) ? activeSet.Count : 0;
		}

		private Vector3 GenerateRandomPosition()
		{
			return new Vector3(
				UnityEngine.Random.Range(-_levelSize.x / 2, _levelSize.x / 2),
				UnityEngine.Random.Range(-_levelSize.y / 2, _levelSize.y / 2),
				0
			);
		}

		private async UniTask LoadEnemyResources(EnemyType enemyType, CancellationToken cancellationToken)
		{
			// Skip if already loaded
			if (_enemyConfigs.ContainsKey(enemyType))
				return;

			// Load prefab
			string prefabAddressableKey = GetAddressableKeyForEnemyType(enemyType);
			GameObject prefab = await _assetsProvider.LoadAssetAsync<GameObject>(prefabAddressableKey);
			_enemyPrefabs[enemyType] = prefab;

			// Load config
			string configAddressableKey = $"EnemyConfigs/{enemyType}Config";
			EnemyConfig config = await _assetsProvider.LoadAssetAsync<EnemyConfig>(configAddressableKey);

			if (config != null)
			{
				_enemyConfigs[enemyType] = config;
			}
			else
			{
				Debug.LogError($"Failed to load enemy config for type: {enemyType}");
			}
		}

		private void CreatePoolForEnemyType(EnemyType enemyType, int maxSize)
		{
			if (!_enemyPrefabs.TryGetValue(enemyType, out var prefab) ||
				!_enemyConfigs.TryGetValue(enemyType, out var config))
			{
				Debug.LogError($"Resources not loaded for enemy type: {enemyType}");
				return;
			}

			// Reuse existing pool if it exists
			if (_enemyPools.ContainsKey(enemyType))
			{
				return;
			}

			var pool = new ObjectPool<Enemy>(
				createFunc: () => CreateEnemy(prefab, config),
				actionOnGet: enemy => ActivateEnemy(enemy),
				actionOnRelease: enemy => DeactivateEnemy(enemy),
				actionOnDestroy: enemy => DestroyEnemy(enemy),
				defaultCapacity: 5,
				maxSize: maxSize
			);

			_enemyPools[enemyType] = pool;
		}

		private Enemy CreateEnemy(GameObject prefab, EnemyConfig config)
		{
			GameObject instance = UnityEngine.Object.Instantiate(prefab);
			Enemy enemy = instance.GetComponent<Enemy>();
			enemy.SetConfig(config);
			return enemy;
		}

		private void ActivateEnemy(Enemy enemy)
		{
			enemy.GameObject.SetActive(true);
			enemy.Initialize(OnEnemyDeath);
		}

		private void DeactivateEnemy(Enemy enemy)
		{
			enemy.GameObject.SetActive(false);
		}

		private void DestroyEnemy(Enemy enemy)
		{
			if (enemy?.GameObject != null)
				UnityEngine.Object.Destroy(enemy.GameObject);
		}

		private string GetAddressableKeyForEnemyType(EnemyType enemyType)
		{
			return enemyType switch
			{
				EnemyType.Basic => "Enemies/BasicEnemy",
				EnemyType.Fast => "Enemies/FastEnemy",
				EnemyType.Tank => "Enemies/TankEnemy",
				EnemyType.Ranged => "Enemies/RangedEnemy",
				EnemyType.Boss => "Enemies/BossEnemy",
				_ => throw new ArgumentOutOfRangeException(nameof(enemyType), enemyType, "Unknown enemy type")
			};
		}

		private int GetMaxPoolSizeForEnemyType(EnemyType enemyType, IReadOnlyList<EnemySpawnConfig> spawnConfigs)
		{
			int maxSize = 5; // Default minimum pool size

			foreach (var config in spawnConfigs)
			{
				if (config.EnemyType == enemyType && config.MaxEnemies > maxSize)
				{
					maxSize = config.MaxEnemies;
				}
			}

			return maxSize;
		}

		private void OnEnemyDeath(IEnemy enemy, int points)
		{
			_eventBus.Fire(new EnemyKilledEvent(points));

			if (enemy is Enemy enemyComponent)
			{
				RemoveFromActiveEnemies(enemyComponent);
				ReleaseEnemyToPool(enemyComponent);
			}
		}

		private void RemoveFromActiveEnemies(Enemy enemy)
		{
			foreach (var pair in _activeEnemies)
			{
				var activeSet = pair.Value;
				if (activeSet.Contains(enemy))
				{
					activeSet.Remove(enemy);
					break;
				}
			}
		}

		private void ReleaseEnemyToPool(Enemy enemy)
		{
			foreach (var pool in _enemyPools.Values)
			{
				pool.Release(enemy);
				break;
			}
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			CancelAllSpawnTasks();
			ClearActiveEnemies();
			ClearPools();
			ReleaseAssets();

			_isDisposed = true;
		}

		private void CancelAllSpawnTasks()
		{
			foreach (var cts in _spawnTaskCts)
			{
				cts.Cancel();
				cts.Dispose();
			}
			_spawnTaskCts.Clear();
		}

		private void ClearActiveEnemies()
		{
			_activeEnemies.Clear();
		}

		private void ClearPools()
		{
			foreach (var pool in _enemyPools.Values)
			{
				pool.Clear();
			}
			_enemyPools.Clear();
		}

		private void ReleaseAssets()
		{
			foreach (var prefab in _enemyPrefabs.Values)
			{
				_assetsProvider.Release(prefab);
			}
			_enemyPrefabs.Clear();

			foreach (var config in _enemyConfigs.Values)
			{
				_assetsProvider.Release(config);
			}
			_enemyConfigs.Clear();
		}
	}
}