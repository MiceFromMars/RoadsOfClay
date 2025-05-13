using ROC.UI.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.MainMenu
{
	public class MainMenuView : BaseView, IMainMenuView
	{
		[SerializeField] private Button _playButton;
		[SerializeField] private Button _levelSelectionButton;
		//[SerializeField] private Button _quitButton;

		private Action _onPlayButtonClicked;
		private Action _onLevelSelectionButtonClicked;
		private Action _onQuitButtonClicked;

		public GameObject GameObject => gameObject;

		protected override void InitializeView()
		{
			_playButton.onClick.AddListener(HandlePlayButtonClicked);
			_levelSelectionButton.onClick.AddListener(HandleLevelSelectionButtonClicked);
			//_quitButton.onClick.AddListener(HandleQuitButtonClicked);
		}

		protected override void OnDestroy()
		{
			_playButton.onClick.RemoveListener(HandlePlayButtonClicked);
			_levelSelectionButton.onClick.RemoveListener(HandleLevelSelectionButtonClicked);
			//_quitButton.onClick.RemoveListener(HandleQuitButtonClicked);

			base.OnDestroy();
		}

		public void SetPlayButtonClickListener(Action callback)
		{
			_onPlayButtonClicked = callback;
		}

		public void SetLevelSelectionButtonClickListener(Action callback)
		{
			_onLevelSelectionButtonClicked = callback;
		}

		public void SetQuitButtonClickListener(Action callback)
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