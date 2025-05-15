using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.Game.PlayerInput;
using ROC.UI.Common;
using VContainer;

namespace ROC.UI.HUD
{
	public class GameHUDPresenter : BasePresenter<IGameHUDView>
	{
		private readonly IInputProvider _inputProvider;

		public int CurrentLives { get; private set; }
		public int MaxLives { get; private set; }
		public int Score { get; private set; }
		public float Height { get; private set; }
		public float Speed { get; private set; }

		[Inject]
		public GameHUDPresenter(
			IGameHUDView view,
			IEventBus eventBus,
			IInputProvider inputProvider) : base(view, eventBus)
		{
			_inputProvider = inputProvider;

			// Initialize default values
			CurrentLives = 3;
			MaxLives = 3;
		}

		public override void Initialize()
		{
			base.Initialize();

			// Connect view events
			_view.SetLeftButtonListeners(OnLeftButtonPressedHandler, OnLeftButtonReleasedHandler);
			_view.SetRightButtonListeners(OnRightButtonPressedHandler, OnRightButtonReleasedHandler);
			_view.SetJumpButtonListeners(OnJumpButtonPressedHandler, OnJumpButtonReleasedHandler);

			// Initialize view with data
			_view.UpdateLives(CurrentLives, MaxLives);
			_view.UpdateScore(Score);
			_view.UpdateHeight(Height);
			_view.UpdateSpeed(Speed);
		}

		protected override void SubscribeToEvents()
		{
			base.SubscribeToEvents();

			_eventBus.Subscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
			_eventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
			_eventBus.Subscribe<HeightChangedEvent>(OnHeightChanged);
			_eventBus.Subscribe<SpeedChangedEvent>(OnSpeedChanged);
		}

		protected override void UnsubscribeFromEvents()
		{
			base.UnsubscribeFromEvents();

			_eventBus.Unsubscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
			_eventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
			_eventBus.Unsubscribe<HeightChangedEvent>(OnHeightChanged);
			_eventBus.Unsubscribe<SpeedChangedEvent>(OnSpeedChanged);
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			await base.Show(cancellationToken);
			_eventBus.Fire(new GameScreenOpenedEvent());
		}

		// Event handlers for view interactions
		private void OnLeftButtonPressedHandler()
		{
			_inputProvider.HandleHUDInput(InputType.Left, true);
		}

		private void OnLeftButtonReleasedHandler()
		{
			_inputProvider.HandleHUDInput(InputType.Left, false);
		}

		private void OnRightButtonPressedHandler()
		{
			_inputProvider.HandleHUDInput(InputType.Right, true);
		}

		private void OnRightButtonReleasedHandler()
		{
			_inputProvider.HandleHUDInput(InputType.Right, false);
		}

		private void OnJumpButtonPressedHandler()
		{
			_inputProvider.HandleHUDInput(InputType.Jump, true);
		}

		private void OnJumpButtonReleasedHandler()
		{
			_inputProvider.HandleHUDInput(InputType.Jump, false);
		}

		// Event handlers for game state updates
		private void OnPlayerLivesChanged(PlayerLivesChangedEvent evt)
		{
			CurrentLives = evt.CurrentLives;
			MaxLives = evt.MaxLives;
			_view.UpdateLives(CurrentLives, MaxLives);
		}

		private void OnScoreChanged(ScoreChangedEvent evt)
		{
			Score = evt.Score;
			_view.UpdateScore(Score);
		}

		private void OnHeightChanged(HeightChangedEvent evt)
		{
			Height = evt.Height;
			_view.UpdateHeight(Height);
		}

		private void OnSpeedChanged(SpeedChangedEvent evt)
		{
			Speed = evt.Speed;
			_view.UpdateSpeed(Speed);
		}
	}
}