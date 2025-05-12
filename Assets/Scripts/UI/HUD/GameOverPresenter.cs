using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.UI.Common;

namespace ROC.UI.HUD
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

		protected override void SubscribeToEvents()
		{
			base.SubscribeToEvents();

			_eventBus.Subscribe<GameOverScreenOpenedEvent>(OnGameOverScreenOpened);
		}

		protected override void UnsubscribeFromEvents()
		{
			base.UnsubscribeFromEvents();

			_eventBus.Unsubscribe<GameOverScreenOpenedEvent>(OnGameOverScreenOpened);
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			// Update view with current data
			_view.SetResult(IsWin);
			_view.SetScore(Score);
			_view.SetLevel(LevelIndex);

			await base.Show(cancellationToken);
		}

		private void OnGameOverScreenOpened(GameOverScreenOpenedEvent evt)
		{
			IsWin = evt.IsWin;
			Score = evt.Score;
			LevelIndex = evt.LevelIndex;

			_view.SetResult(IsWin);
			_view.SetScore(Score);
			_view.SetLevel(LevelIndex);
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