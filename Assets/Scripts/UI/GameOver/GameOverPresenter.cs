using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.UI.Common;

namespace ROC.UI.GameOver
{
	public class GameOverPresenter : BasePresenter<IGameOverView>
	{
		public bool IsWin { get; private set; }
		public int Score { get; private set; }
		public int LevelIndex { get; private set; }

		public GameOverPresenter(
			IGameOverView view,
			IEventBus eventBus) : base(view, eventBus)
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			// Set up view event listeners
			_view.SetRestartButtonListener(OnRestartClicked);
			_view.SetNextLevelButtonListener(OnNextLevelClicked);
			_view.SetMainMenuButtonListener(OnMainMenuClicked);
		}

		public void SetGameOverData(bool isWin, int score, int levelIndex)
		{
			IsWin = isWin;
			Score = score;
			LevelIndex = levelIndex;

			// Fire the event instead of subscribing to it
			_eventBus.Fire(new GameOverScreenOpenedEvent
			{
				IsWin = IsWin,
				Score = Score,
				LevelIndex = LevelIndex
			});

			// Update view with current data
			_view.SetResult(IsWin);
			_view.SetScore(Score);
			_view.SetLevel(LevelIndex);
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			// Update view with current data already done in SetGameOverData
			await base.Show(cancellationToken);
		}

		private void OnRestartClicked()
		{
			_eventBus.Fire(new RestartLevelEvent { LevelIndex = LevelIndex });
		}

		private void OnNextLevelClicked()
		{
			_eventBus.Fire(new NextLevelEvent { LevelIndex = LevelIndex + 1 });
		}

		private void OnMainMenuClicked()
		{
			_eventBus.Fire(new ReturnToMainMenuEvent());
		}
	}
}