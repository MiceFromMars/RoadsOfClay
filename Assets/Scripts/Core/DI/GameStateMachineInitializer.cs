using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.StateMachine;
using ROC.Core.StateMachine.States;
using VContainer.Unity;
using ROC.Core.Assets;

namespace ROC.Core.DI
{
	public class GameStateMachineInitializer : IAsyncStartable
	{
		private readonly GameStateMachine _stateMachine;
		private readonly ILoggingService _logger;
		private readonly BootstrapState _bootstrapState;
		private readonly MainMenuState _mainMenuState;
		private readonly GameplayState _gameplayState;

		public GameStateMachineInitializer(
			GameStateMachine stateMachine,
			ILoggingService logger,
			BootstrapState bootstrapState,
			MainMenuState mainMenuState,
			GameplayState gameplayState)
		{
			_stateMachine = stateMachine;
			_logger = logger;
			_bootstrapState = bootstrapState;
			_mainMenuState = mainMenuState;
			_gameplayState = gameplayState;
		}

		public async UniTask StartAsync(CancellationToken cancellation)
		{
			_logger.Log("GameStateMachineInitializer starting...");

			// Set GameStateMachine properties on all states
			_bootstrapState.StateMachine = _stateMachine;
			_mainMenuState.StateMachine = _stateMachine;
			_gameplayState.StateMachine = _stateMachine;

			// Register all states
			_stateMachine.RegisterState(_bootstrapState);
			_stateMachine.RegisterState(_mainMenuState);
			_stateMachine.RegisterState(_gameplayState);

			_logger.Log("States registered and initialized. Starting from bootstrap state...");

			await _stateMachine.Enter<BootstrapState>();
		}
	}
}