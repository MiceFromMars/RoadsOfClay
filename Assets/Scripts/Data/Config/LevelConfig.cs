using System.Collections.Generic;
using UnityEngine;
using ROC.Game.Enemy;

namespace ROC.Data.Config
{
	[CreateAssetMenu(fileName = "LevelConfig", menuName = "ROC/Config/LevelConfig")]
	public class LevelConfig : ScriptableObject
	{
		[SerializeField] private string _levelName;
		[SerializeField] private int _scoreToUnlock;
		[SerializeField] private List<EnemySpawnConfig> _enemySpawnConfigs;

		public string LevelName => _levelName;
		public int ScoreToUnlock => _scoreToUnlock;
		public IReadOnlyList<EnemySpawnConfig> EnemySpawnConfigs => _enemySpawnConfigs;
	}

	[System.Serializable]
	public class EnemySpawnConfig
	{
		[SerializeField] private EnemyType _enemyType;
		[SerializeField] private float _startSpawnDelay;
		[SerializeField] private float _spawnInterval;
		[SerializeField] private int _maxEnemies;

		public EnemyType EnemyType => _enemyType;
		public float StartSpawnDelay => _startSpawnDelay;
		public float SpawnInterval => _spawnInterval;
		public int MaxEnemies => _maxEnemies;
	}
}