using System;
using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.Data.Config;
using UnityEngine;
using VContainer;

namespace ROC.Game.Player
{
	public class PlayerBehavior : MonoBehaviour, IDisposable
	{
		[SerializeField] private Rigidbody2D _rigidbody;
		[SerializeField] private BoxCollider2D _collider;
		[SerializeField] private Transform _bulletSpawnPoint;
		[SerializeField] private float _groundCheckDistance = 0.1f;
		[SerializeField] private LayerMask _groundLayer;

		private PlayerConfig _playerConfig;
		private IEventBus _eventBus;
		private ILoggingService _logger;

		private int _lives;
		private int _score;
		private float _maxHeight;
		private float _maxSpeed;
		private bool _isGrounded;
		private bool _isDisposed;

		private float _horizontalInput;
		private bool _jumpInput;

		// This constructor will not be used when instantiating from prefab
		// It's kept for reference but commented out
		/*
		public PlayerBehavior(
			PlayerConfig playerConfig,
			IEventBus eventBus,
			ILoggingService logger)
		{
			_playerConfig = playerConfig ?? throw new ArgumentNullException(nameof(playerConfig));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
		*/

		public void Initialize(PlayerConfig playerConfig)
		{
			_playerConfig = playerConfig ?? throw new ArgumentNullException(nameof(playerConfig));

			// Get references from container
			var container = VContainer.Unity.VContainerSettings.Instance.RootLifetimeScope.Container;
			_eventBus = container.Resolve<IEventBus>();
			_logger = container.Resolve<ILoggingService>();

			ValidateComponents();
			ResetPlayer();
		}

		public void Cleanup()
		{
			Dispose();
		}

		private void Awake()
		{
			ValidateComponents();
		}

		private void Start()
		{
			if (_playerConfig != null)
			{
				ResetPlayer();
			}
		}

		private void ValidateComponents()
		{
			if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
			if (_collider == null) _collider = GetComponent<BoxCollider2D>();

			if (_rigidbody == null || _collider == null)
			{
				_logger.LogError("Required components missing on PlayerBehavior");
			}
		}

		public void ResetPlayer()
		{
			_lives = _playerConfig.StartLives;
			_score = 0;
			_maxHeight = 0;
			_maxSpeed = 0;
		}

		public void SetInput(float horizontal, bool jump)
		{
			_horizontalInput = horizontal;
			_jumpInput = jump;
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
			_eventBus.Fire(new PlayerDiedEvent(_score, _maxHeight, _maxSpeed));
			gameObject.SetActive(false);
		}

		private void Update()
		{
			if (_isDisposed) return;

			CheckGrounded();

			// Track statistics
			float currentHeight = transform.position.y;
			float currentSpeed = _rigidbody.linearVelocity.magnitude;

			if (currentHeight > _maxHeight)
			{
				_maxHeight = currentHeight;
				_eventBus.Fire(new MaxHeightChangedEvent(_maxHeight));
			}

			if (currentSpeed > _maxSpeed)
			{
				_maxSpeed = currentSpeed;
				_eventBus.Fire(new MaxSpeedChangedEvent(_maxSpeed));
			}
		}

		private void FixedUpdate()
		{
			if (_isDisposed) return;

			// Horizontal movement
			Vector2 velocity = _rigidbody.linearVelocity;
			velocity.x = _horizontalInput * _playerConfig.MoveSpeed;

			// Jump
			if (_jumpInput && _isGrounded)
			{
				velocity.y = _playerConfig.JumpForce;
				_jumpInput = false;
			}

			_rigidbody.linearVelocity = velocity;
		}

		private void CheckGrounded()
		{
			RaycastHit2D hit = Physics2D.BoxCast(
				_collider.bounds.center,
				_collider.bounds.size,
				0f,
				Vector2.down,
				_groundCheckDistance,
				_groundLayer);

			_isGrounded = hit.collider != null;
		}

		public void Dispose()
		{
			if (_isDisposed) return;

			_isDisposed = true;

			// Unsubscribe from any events if needed

			// Clean up resources
			_horizontalInput = 0;
			_jumpInput = false;
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}