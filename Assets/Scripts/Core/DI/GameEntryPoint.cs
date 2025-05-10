using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Core.StateMachine;
using ROC.Core.StateMachine.States;
using VContainer.Unity;

namespace ROC.Core.DI
{
	public class GameEntryPoint : IAsyncStartable
	{
		private readonly GameStateMachine _stateMachine;
		private readonly IAssetsProvider _assetsProvider;

		public GameEntryPoint(
			GameStateMachine stateMachine,
			IAssetsProvider assetsProvider)
		{
			_stateMachine = stateMachine;
			_assetsProvider = assetsProvider;
		}

		public async UniTask StartAsync(CancellationToken cancellation)
		{
			// Initialize asset provider
			await _assetsProvider.InitializeAsync();

			// Start the game state machine
			await _stateMachine.Enter<BootstrapState>();
		}
	}
}