using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.HUD
{
	public class GameHUD : UIScreen
	{
		[Header("Controls")]
		[SerializeField] private UIInputButton _leftButton;
		[SerializeField] private UIInputButton _rightButton;
		[SerializeField] private UIInputButton _jumpButton;

		[Header("Lives")]
		[SerializeField] private GameObject _lifeImagePrefab;
		[SerializeField] private Transform _livesContainer;
		[SerializeField] private int _maxLives = 3;

		[Header("Stats")]
		[SerializeField] private TextMeshProUGUI _scoreText;
		[SerializeField] private TextMeshProUGUI _heightText;
		[SerializeField] private TextMeshProUGUI _speedText;

		private readonly IEventBus _eventBus;
		private readonly List<GameObject> _lifeImages = new List<GameObject>();
		private int _currentLives;
		private int _score;
		private float _height;
		private float _speed;

		// Input events
		public event Action OnLeftButtonPressed;
		public event Action OnLeftButtonReleased;
		public event Action OnRightButtonPressed;
		public event Action OnRightButtonReleased;
		public event Action OnJumpButtonPressed;
		public event Action OnJumpButtonReleased;

		public GameHUD(IEventBus eventBus)
		{
			_eventBus = eventBus;
			_currentLives = _maxLives;
		}

		protected override void Awake()
		{
			base.Awake();

			SetupButtons();
			InitializeLives();
			ResetStats();

			_eventBus.Subscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
			_eventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
			_eventBus.Subscribe<HeightChangedEvent>(OnHeightChanged);
			_eventBus.Subscribe<SpeedChangedEvent>(OnSpeedChanged);
		}

		protected override void OnDestroy()
		{
			CleanupButtons();

			_eventBus.Unsubscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
			_eventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
			_eventBus.Unsubscribe<HeightChangedEvent>(OnHeightChanged);
			_eventBus.Unsubscribe<SpeedChangedEvent>(OnSpeedChanged);

			base.OnDestroy();
			Dispose();
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			await base.Show(cancellationToken);
			_eventBus.Fire(new GameScreenOpenedEvent());
		}

		private void SetupButtons()
		{
			// Setup control buttons with pressed/released events for movement control
			_leftButton.OnPress += HandleLeftButtonPressed;
			_leftButton.OnRelease += HandleLeftButtonReleased;

			_rightButton.OnPress += HandleRightButtonPressed;
			_rightButton.OnRelease += HandleRightButtonReleased;

			_jumpButton.OnPress += HandleJumpButtonPressed;
			_jumpButton.OnRelease += HandleJumpButtonReleased;
		}

		private void CleanupButtons()
		{
			_leftButton.OnPress -= HandleLeftButtonPressed;
			_leftButton.OnRelease -= HandleLeftButtonReleased;

			_rightButton.OnPress -= HandleRightButtonPressed;
			_rightButton.OnRelease -= HandleRightButtonReleased;

			_jumpButton.OnPress -= HandleJumpButtonPressed;
			_jumpButton.OnRelease -= HandleJumpButtonReleased;
		}

		private void HandleLeftButtonPressed() => OnLeftButtonPressed?.Invoke();
		private void HandleLeftButtonReleased() => OnLeftButtonReleased?.Invoke();
		private void HandleRightButtonPressed() => OnRightButtonPressed?.Invoke();
		private void HandleRightButtonReleased() => OnRightButtonReleased?.Invoke();
		private void HandleJumpButtonPressed() => OnJumpButtonPressed?.Invoke();
		private void HandleJumpButtonReleased() => OnJumpButtonReleased?.Invoke();

		private void InitializeLives()
		{
			// Clear existing lives first
			foreach (var lifeImage in _lifeImages)
			{
				Destroy(lifeImage);
			}
			_lifeImages.Clear();

			// Create life images
			for (int i = 0; i < _maxLives; i++)
			{
				GameObject lifeImage = Instantiate(_lifeImagePrefab, _livesContainer);
				_lifeImages.Add(lifeImage);
			}

			UpdateLivesDisplay();
		}

		private void UpdateLivesDisplay()
		{
			for (int i = 0; i < _lifeImages.Count; i++)
			{
				_lifeImages[i].SetActive(i < _currentLives);
			}
		}

		private void ResetStats()
		{
			_score = 0;
			_height = 0;
			_speed = 0;

			UpdateStatsDisplay();
		}

		private void UpdateStatsDisplay()
		{
			_scoreText.text = $"Score: {_score}";
			_heightText.text = $"Height: {_height:0.0}m";
			_speedText.text = $"Speed: {_speed:0.0}m/s";
		}

		private void OnPlayerLivesChanged(PlayerLivesChangedEvent evt)
		{
			_currentLives = evt.CurrentLives;
			_maxLives = evt.MaxLives;

			// Ensure we have the correct number of life images
			if (_maxLives != _lifeImages.Count)
			{
				InitializeLives();
			}
			else
			{
				UpdateLivesDisplay();
			}
		}

		private void OnScoreChanged(ScoreChangedEvent evt)
		{
			_score = evt.Score;
			UpdateStatsDisplay();
		}

		private void OnHeightChanged(HeightChangedEvent evt)
		{
			_height = evt.Height;
			UpdateStatsDisplay();
		}

		private void OnSpeedChanged(SpeedChangedEvent evt)
		{
			_speed = evt.Speed;
			UpdateStatsDisplay();
		}
	}
}