using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.StateMachine;
using ROC.Core.StateMachine.States;
using VContainer.Unity;

namespace ROC.Core.DI
{
	public class GameStateMachineInitializer : IAsyncStartable
	{
		private readonly GameStateMachine _stateMachine;

		public GameStateMachineInitializer(GameStateMachine stateMachine)
		{
			_stateMachine = stateMachine;
		}

		public async UniTask StartAsync(CancellationToken cancellation)
		{
			await _stateMachine.Enter<BootstrapState>();
		}
	}
}