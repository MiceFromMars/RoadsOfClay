using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using VContainer.Unity;

namespace ROC.Core.StateMachine
{
	public class GameStateMachine : IDisposable
	{
		private readonly Dictionary<Type, IState> _states;
		private readonly ILoggingService _logger;
		private IState _currentState;
		private CancellationTokenSource _stateCts;

		public GameStateMachine(IReadOnlyList<IState> states, ILoggingService logger)
		{
			_states = new Dictionary<Type, IState>();
			_logger = logger;

			foreach (IState state in states)
				_states.Add(state.GetType(), state);

			_stateCts = new CancellationTokenSource();
		}

		public async UniTask Enter<TState>() where TState : class, IState
		{
			TState newState = await ChangeStateAsync<TState>();
			await newState.Enter(_stateCts.Token);
		}

		public async UniTask Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
		{
			TState newState = await ChangeStateAsync<TState>();
			await newState.Enter(payload, _stateCts.Token);
		}

		private async UniTask<TState> ChangeStateAsync<TState>() where TState : class, IState
		{
			_stateCts?.Cancel();
			_stateCts = new CancellationTokenSource();

			if (_currentState != null)
			{
				try
				{
					// Properly await the Exit operation to avoid race conditions
					await _currentState.Exit(_stateCts.Token);
				}
				catch (Exception ex)
				{
					_logger.LogException(ex, "State exit");
				}
			}

			TState state = GetState<TState>();
			_currentState = state;

			return state;
		}

		private TState GetState<TState>() where TState : class, IState
		{
			return _states[typeof(TState)] as TState;
		}

		public void Dispose()
		{
			_stateCts?.Cancel();
			_stateCts?.Dispose();
		}
	}
}