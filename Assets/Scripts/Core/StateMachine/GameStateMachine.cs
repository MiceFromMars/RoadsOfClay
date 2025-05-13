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
		private Dictionary<Type, IState> _states;
		private readonly ILoggingService _logger;
		private IState _currentState;
		private CancellationTokenSource _stateCts;

		// New constructor that doesn't require states upfront
		public GameStateMachine(ILoggingService logger)
		{
			_states = new Dictionary<Type, IState>();
			_logger = logger;
			_stateCts = new CancellationTokenSource();
		}

		// Original constructor - kept for backward compatibility
		public GameStateMachine(IReadOnlyList<IState> states, ILoggingService logger)
		{
			_states = new Dictionary<Type, IState>();
			_logger = logger;

			foreach (IState state in states)
				_states.Add(state.GetType(), state);

			_stateCts = new CancellationTokenSource();
		}

		// Method to register states after construction
		public void RegisterState(IState state)
		{
			if (state == null)
				throw new ArgumentNullException(nameof(state));

			Type stateType = state.GetType();

			if (_states.ContainsKey(stateType))
				_states[stateType] = state;  // Replace existing
			else
				_states.Add(stateType, state); // Add new
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
			Type stateType = typeof(TState);

			if (!_states.ContainsKey(stateType))
			{
				_logger.LogError($"State of type {stateType.Name} is not registered in the GameStateMachine.");
				throw new KeyNotFoundException($"The given key '{stateType.FullName}' was not present in the dictionary.");
			}

			return _states[stateType] as TState;
		}

		public void Dispose()
		{
			_stateCts?.Cancel();
			_stateCts?.Dispose();
		}
	}
}