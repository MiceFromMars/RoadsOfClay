using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.MainMenu
{
	public class MainMenuScreen : UIScreen
	{
		[SerializeField] private UIButton _playButton;
		[SerializeField] private UIButton _levelSelectionButton;
		[SerializeField] private UIButton _quitButton;

		private readonly IEventBus _eventBus;

		public MainMenuScreen(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		protected override void Awake()
		{
			base.Awake();

			_playButton.Button.onClick.AddListener(OnPlayButtonClicked);
			_levelSelectionButton.Button.onClick.AddListener(OnLevelSelectionButtonClicked);
			_quitButton.Button.onClick.AddListener(OnQuitButtonClicked);
		}

		protected override void OnDestroy()
		{
			_playButton.Button.onClick.RemoveListener(OnPlayButtonClicked);
			_levelSelectionButton.Button.onClick.RemoveListener(OnLevelSelectionButtonClicked);
			_quitButton.Button.onClick.RemoveListener(OnQuitButtonClicked);

			base.OnDestroy();
			Dispose();
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			await base.Show(cancellationToken);
			_eventBus.Fire(new MainMenuOpenedEvent());
		}

		private void OnPlayButtonClicked()
		{
			_eventBus.Fire(new LevelSelectedEvent { LevelIndex = 0 });
		}

		private void OnLevelSelectionButtonClicked()
		{
			_eventBus.Fire(new LevelSelectionOpenedEvent());
		}

		private void OnQuitButtonClicked()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}