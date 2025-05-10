using System;
using ROC.Data.Config;
using UnityEngine;
using ROC.Game.Player;

namespace ROC.Game.Enemy
{
	public class Enemy : MonoBehaviour, IEnemy
	{
		private EnemyConfig _config;
		private int _currentHealth;
		private Action<IEnemy, int> _onDeathCallback;
		private Transform _target;

		public GameObject GameObject => gameObject;

		public void SetConfig(EnemyConfig config)
		{
			_config = config;
			_currentHealth = config.Health;
		}

		public void Initialize(Action<IEnemy, int> onDeathCallback)
		{
			_onDeathCallback = onDeathCallback;
			_currentHealth = _config.Health; // Reset health when reused from pool

			// Find player as target
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if (player != null)
				_target = player.transform;
		}

		public void TakeDamage(int damage)
		{
			_currentHealth -= damage;

			if (_currentHealth <= 0)
				Die();
		}

		public void SetPosition(Vector3 position)
		{
			transform.position = position;
		}

		private void Die()
		{
			_onDeathCallback?.Invoke(this, _config.Points);
		}

		private void Update()
		{
			if (_target == null || _config == null)
				return;

			// Simple follow behavior
			Vector3 direction = (_target.position - transform.position).normalized;
			transform.position += direction * _config.MoveSpeed * Time.deltaTime;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.CompareTag("Player"))
			{
				if (collision.gameObject.TryGetComponent(out PlayerBehavior player))
				{
					player.TakeDamage(1);
				}
			}
		}
	}
}