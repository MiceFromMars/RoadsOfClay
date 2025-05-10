using UnityEngine;
using ROC.Game.Enemy;

namespace ROC.Data.Config
{
	[CreateAssetMenu(fileName = "EnemyConfig", menuName = "ROC/Config/EnemyConfig")]
	public class EnemyConfig : ScriptableObject
	{
		[SerializeField] private EnemyType _enemyType;
		[SerializeField] private float _moveSpeed = 2f;
		[SerializeField] private int _health = 3;
		[SerializeField] private int _points = 10;

		public EnemyType EnemyType => _enemyType;
		public float MoveSpeed => _moveSpeed;
		public int Health => _health;
		public int Points => _points;
	}
}