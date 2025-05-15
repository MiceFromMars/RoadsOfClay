using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.UI.Common;
using UnityEngine;

namespace ROC.UI.MainMenu
{
	public class MainMenuPresenter : BasePresenter<IMainMenuView>
	{
		public MainMenuPresenter(
			IMainMenuView view,
			IEventBus eventBus) : base(view, eventBus)
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			// Set up listeners for view actions
			_view.SetPlayButtonClickListener(OnPlayButtonClicked);
			_view.SetLevelSelectionButtonClickListener(OnLevelSelectionButtonClicked);
			//_view.SetQuitButtonClickListener(OnQuitButtonClicked);
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			await base.Show(cancellationToken);
			_eventBus.Fire(new MainMenuOpenedEvent());
		}

		private void OnPlayButtonClicked()
		{
			Debug.Log("MainMenuPresenter: Play button clicked");
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