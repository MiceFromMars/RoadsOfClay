using ROC.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.HUD
{
	public interface IGameOverView : IView
	{
		void SetResult(bool isWin);
		void SetScore(int score);
		void SetLevel(int level);
		void SetRestartButtonListener(System.Action callback);
		void SetNextLevelButtonListener(System.Action callback);
		void SetMainMenuButtonListener(System.Action callback);
	}

	public class GameOverView : BaseView, IGameOverView
	{
		[SerializeField] private GameObject _winContainer;
		[SerializeField] private GameObject _loseContainer;
		[SerializeField] private TextMeshProUGUI _scoreText;
		[SerializeField] private TextMeshProUGUI _levelText;
		[SerializeField] private Button _restartButton;
		[SerializeField] private Button _nextLevelButton;
		[SerializeField] private Button _mainMenuButton;

		private System.Action _onRestartButtonClicked;
		private System.Action _onNextLevelButtonClicked;
		private System.Action _onMainMenuButtonClicked;

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

		public void SetRestartButtonListener(System.Action callback)
		{
			_onRestartButtonClicked = callback;
		}

		public void SetNextLevelButtonListener(System.Action callback)
		{
			_onNextLevelButtonClicked = callback;
		}

		public void SetMainMenuButtonListener(System.Action callback)
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