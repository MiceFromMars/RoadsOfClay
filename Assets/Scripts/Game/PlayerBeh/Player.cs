using System;
using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.Data.Config;
using UnityEngine;
using VContainer;

namespace ROC.Game.PlayerBeh
{
	public class Player : MonoBehaviour
	{
		private PlayerConfig _playerConfig;
		private IEventBus _eventBus;
		private ILoggingService _logger;

		private int _lives;
		private int _score;

		[Inject]
		public void Initialize(PlayerConfig playerConfig, IEventBus eventBus, ILoggingService logger)
		{
			_playerConfig = playerConfig;
			_eventBus = eventBus;
			_logger = logger;
		}

		public void ResetPlayer(Vector3 spawnPosition)
		{
			_lives = _playerConfig.StartLives;
			transform.position = spawnPosition;
			gameObject.SetActive(true);
			_score = 0;
		}

		public void TakeDamage(int damage)
		{
			_lives -= damage;

			_eventBus.Fire(new PlayerDamagedEvent(_lives));

			if (_lives <= 0)
				Die();
		}

		public void AddScore(int points)
		{
			_score += points;
			_eventBus.Fire(new ScoreChangedEvent(_score));
		}

		private void Die()
		{
			// _eventBus.Fire(new PlayerDiedEvent(_score, _maxHeight, _maxSpeed));
		}

		public void Cleanup()
		{
			// Cleanup method to implement IDisposable pattern
		}
	}
}