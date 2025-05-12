using ROC.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.MainMenu
{
	public interface IMainMenuView : IView
	{
		void SetPlayButtonClickListener(System.Action callback);
		void SetLevelSelectionButtonClickListener(System.Action callback);
		void SetQuitButtonClickListener(System.Action callback);
	}

	public class MainMenuView : BaseView, IMainMenuView
	{
		[SerializeField] private Button _playButton;
		[SerializeField] private Button _levelSelectionButton;
		[SerializeField] private Button _quitButton;

		private System.Action _onPlayButtonClicked;
		private System.Action _onLevelSelectionButtonClicked;
		private System.Action _onQuitButtonClicked;

		protected override void InitializeView()
		{
			_playButton.onClick.AddListener(HandlePlayButtonClicked);
			_levelSelectionButton.onClick.AddListener(HandleLevelSelectionButtonClicked);
			_quitButton.onClick.AddListener(HandleQuitButtonClicked);
		}

		protected override void OnDestroy()
		{
			_playButton.onClick.RemoveListener(HandlePlayButtonClicked);
			_levelSelectionButton.onClick.RemoveListener(HandleLevelSelectionButtonClicked);
			_quitButton.onClick.RemoveListener(HandleQuitButtonClicked);

			base.OnDestroy();
		}

		public void SetPlayButtonClickListener(System.Action callback)
		{
			_onPlayButtonClicked = callback;
		}

		public void SetLevelSelectionButtonClickListener(System.Action callback)
		{
			_onLevelSelectionButtonClicked = callback;
		}

		public void SetQuitButtonClickListener(System.Action callback)
		{
			_onQuitButtonClicked = callback;
		}

		private void HandlePlayButtonClicked()
		{
			_onPlayButtonClicked?.Invoke();
		}

		private void HandleLevelSelectionButtonClicked()
		{
			_onLevelSelectionButtonClicked?.Invoke();
		}

		private void HandleQuitButtonClicked()
		{
			_onQuitButtonClicked?.Invoke();
		}
	}
}