using System.Threading;
using Cysharp.Threading.Tasks;

namespace ROC.Core.StateMachine
{
    public interface IPayloadedState<TPayload> : IState
    {
        UniTask Enter(TPayload payload, CancellationToken cancellationToken);
    }
} 