using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ROC.Core.Events;
using ROC.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.HUD
{
	public class GameOverScreen : UIScreen
	{
		[SerializeField] private TextMeshProUGUI _titleText;
		[SerializeField] private TextMeshProUGUI _scoreText;
		[SerializeField] private UIButton _menuButton;
		[SerializeField] private UIButton _restartButton;
		[SerializeField] private UIButton _nextLevelButton;
		[SerializeField] private GameObject _winParticles;
		[SerializeField] private Image _overlay;

		[Header("Win/Lose Setup")]
		[SerializeField] private string _winTitle = "LEVEL COMPLETED!";
		[SerializeField] private string _loseTitle = "GAME OVER";
		[SerializeField] private Color _winTitleColor = Color.green;
		[SerializeField] private Color _loseTitleColor = Color.red;

		private readonly IEventBus _eventBus;
		private int _currentLevelIndex;
		private bool _isWin;

		public GameOverScreen(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		protected override void Awake()
		{
			base.Awake();

			_menuButton.Button.onClick.AddListener(OnMenuButtonClicked);
			_restartButton.Button.onClick.AddListener(OnRestartButtonClicked);
			_nextLevelButton.Button.onClick.AddListener(OnNextLevelButtonClicked);
		}

		protected override void OnDestroy()
		{
			_menuButton.Button.onClick.RemoveListener(OnMenuButtonClicked);
			_restartButton.Button.onClick.RemoveListener(OnRestartButtonClicked);
			_nextLevelButton.Button.onClick.RemoveListener(OnNextLevelButtonClicked);

			base.OnDestroy();
			Dispose();
		}

		public void Initialize(bool isWin, int levelIndex, int score)
		{
			_isWin = isWin;
			_currentLevelIndex = levelIndex;

			_titleText.text = isWin ? _winTitle : _loseTitle;
			_titleText.color = isWin ? _winTitleColor : _loseTitleColor;
			_scoreText.text = $"Score: {score}";

			// Only show Next Level button if player won
			_nextLevelButton.gameObject.SetActive(isWin);

			// Show particles only on win
			if (_winParticles != null)
				_winParticles.SetActive(isWin);
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			gameObject.SetActive(true);

			// Start with overlay fade in
			_overlay.color = new Color(_overlay.color.r, _overlay.color.g, _overlay.color.b, 0f);

			// Use the UIExtensions approach for DoTween
			await ROC.UI.Utils.UIExtensions.ColorToAsync(_overlay, new Color(_overlay.color.r, _overlay.color.g, _overlay.color.b, 0.8f), 0.5f, Ease.Linear, cancellationToken);

			// Then animate the UI elements
			await base.Show(cancellationToken);

			// If win, pulse the title text for visual effect
			if (_isWin && _titleText != null)
			{
				_titleText.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
			}

			_eventBus.Fire(new GameOverScreenOpenedEvent
			{
				IsWin = _isWin,
				LevelIndex = _currentLevelIndex,
				Score = int.Parse(_scoreText.text.Replace("Score: ", ""))
			});
		}

		public override async UniTask Hide(CancellationToken cancellationToken = default)
		{
			// Stop any animations
			DOTween.Kill(_titleText.transform);

			await base.Hide(cancellationToken);
		}

		private void OnMenuButtonClicked()
		{
			_eventBus.Fire(new ReturnToMainMenuEvent());
		}

		private void OnRestartButtonClicked()
		{
			_eventBus.Fire(new RestartLevelEvent { LevelIndex = _currentLevelIndex });
		}

		private void OnNextLevelButtonClicked()
		{
			_eventBus.Fire(new NextLevelEvent { LevelIndex = _currentLevelIndex + 1 });
		}
	}
}