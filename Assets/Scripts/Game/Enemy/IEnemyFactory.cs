using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Data.Config;
using UnityEngine;

namespace ROC.Game.Enemy
{
	public interface IEnemyFactory : IDisposable
	{
		UniTask InitializePoolsForLevel(IReadOnlyList<EnemySpawnConfig> spawnConfigs, CancellationToken cancellationToken);
		IEnemy SpawnEnemy(EnemyType enemyType, Vector3 position);
		UniTask StartSpawning(IReadOnlyList<EnemySpawnConfig> spawnConfigs, CancellationToken cancellationToken);
	}
}