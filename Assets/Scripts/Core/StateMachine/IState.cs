using System.Threading;
using Cysharp.Threading.Tasks;

namespace ROC.Core.StateMachine
{
	public interface IState
	{
		UniTask Enter(CancellationToken cancellationToken);
		UniTask Exit(CancellationToken cancellationToken);
	}
}