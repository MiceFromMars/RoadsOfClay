using ROC.UI.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.GameOver
{
	public class GameOverView : BaseView, IGameOverView
	{
		[SerializeField] private GameObject _winContainer;
		[SerializeField] private GameObject _loseContainer;
		[SerializeField] private TextMeshProUGUI _scoreText;
		[SerializeField] private TextMeshProUGUI _levelText;
		[SerializeField] private Button _restartButton;
		[SerializeField] private Button _nextLevelButton;
		[SerializeField] private Button _mainMenuButton;

		private Action _onRestartButtonClicked;
		private Action _onNextLevelButtonClicked;
		private Action _onMainMenuButtonClicked;

		public GameObject GameObject => gameObject;

		protected override void InitializeView()
		{
			_restartButton.onClick.AddListener(HandleRestartButtonClicked);
			_nextLevelButton.onClick.AddListener(HandleNextLevelButtonClicked);
			_mainMenuButton.onClick.AddListener(HandleMainMenuButtonClicked);
		}

		protected override void OnDestroy()
		{
			_restartButton.onClick.RemoveListener(HandleRestartButtonClicked);
			_nextLevelButton.onClick.RemoveListener(HandleNextLevelButtonClicked);
			_mainMenuButton.onClick.RemoveListener(HandleMainMenuButtonClicked);

			base.OnDestroy();
		}

		public void SetResult(bool isWin)
		{
			_winContainer.SetActive(isWin);
			_loseContainer.SetActive(!isWin);
			_nextLevelButton.gameObject.SetActive(isWin);
		}

		public void SetScore(int score)
		{
			_scoreText.text = $"Score: {score}";
		}

		public void SetLevel(int level)
		{
			_levelText.text = $"Level: {level + 1}";
		}

		public void SetRestartButtonListener(Action callback)
		{
			_onRestartButtonClicked = callback;
		}

		public void SetNextLevelButtonListener(Action callback)
		{
			_onNextLevelButtonClicked = callback;
		}

		public void SetMainMenuButtonListener(Action callback)
		{
			_onMainMenuButtonClicked = callback;
		}

		private void HandleRestartButtonClicked()
		{
			_onRestartButtonClicked?.Invoke();
		}

		private void HandleNextLevelButtonClicked()
		{
			_onNextLevelButtonClicked?.Invoke();
		}

		private void HandleMainMenuButtonClicked()
		{
			_onMainMenuButtonClicked?.Invoke();
		}
	}
}