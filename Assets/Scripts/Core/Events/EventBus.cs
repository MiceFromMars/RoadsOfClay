using System;
using System.Collections.Generic;

namespace ROC.Core.Events
{
	public class EventBus : IEventBus
	{
		private readonly Dictionary<Type, List<object>> _subscribers = new Dictionary<Type, List<object>>();
		private readonly Dictionary<Type, bool> _isIterating = new Dictionary<Type, bool>();
		private readonly Dictionary<Type, List<PendingOperation>> _pendingOperations = new Dictionary<Type, List<PendingOperation>>();

		private enum OperationType
		{
			Subscribe,
			Unsubscribe
		}

		private class PendingOperation
		{
			public OperationType Type { get; }
			public object Action { get; }

			public PendingOperation(OperationType type, object action)
			{
				Type = type;
				Action = action;
			}
		}

		public void Subscribe<TEvent>(Action<TEvent> action) where TEvent : struct, IEvent
		{
			Type eventType = typeof(TEvent);

			// If we're currently iterating through this event type, queue the operation
			if (_isIterating.TryGetValue(eventType, out bool isIterating) && isIterating)
			{
				AddPendingOperation(eventType, new PendingOperation(OperationType.Subscribe, action));
				return;
			}

			AddSubscriber(eventType, action);
		}

		public void Unsubscribe<TEvent>(Action<TEvent> action) where TEvent : struct, IEvent
		{
			Type eventType = typeof(TEvent);

			// If we're currently iterating through this event type, queue the operation
			if (_isIterating.TryGetValue(eventType, out bool isIterating) && isIterating)
			{
				AddPendingOperation(eventType, new PendingOperation(OperationType.Unsubscribe, action));
				return;
			}

			RemoveSubscriber(eventType, action);
		}

		public void Fire<TEvent>(TEvent eventData) where TEvent : struct, IEvent
		{
			Type eventType = typeof(TEvent);

			if (!_subscribers.ContainsKey(eventType) || _subscribers[eventType].Count == 0)
				return;

			// Mark that we're iterating through this event type
			_isIterating[eventType] = true;

			// Call all current subscribers
			var subscribers = _subscribers[eventType];
			for (int i = 0; i < subscribers.Count; i++)
			{
				if (subscribers[i] is Action<TEvent> action)
				{
					action.Invoke(eventData);
				}
			}

			// Mark that we're done iterating
			_isIterating[eventType] = false;

			// Process any pending operations for this event type
			ProcessPendingOperations(eventType);
		}

		private void AddPendingOperation(Type eventType, PendingOperation operation)
		{
			if (!_pendingOperations.ContainsKey(eventType))
			{
				_pendingOperations[eventType] = new List<PendingOperation>();
			}

			_pendingOperations[eventType].Add(operation);
		}

		private void ProcessPendingOperations(Type eventType)
		{
			if (!_pendingOperations.ContainsKey(eventType) || _pendingOperations[eventType].Count == 0)
				return;

			foreach (var operation in _pendingOperations[eventType])
			{
				switch (operation.Type)
				{
					case OperationType.Subscribe:
						AddSubscriber(eventType, operation.Action);
						break;
					case OperationType.Unsubscribe:
						RemoveSubscriber(eventType, operation.Action);
						break;
				}
			}

			// Clear the processed operations
			_pendingOperations[eventType].Clear();
		}

		private void AddSubscriber(Type eventType, object action)
		{
			if (!_subscribers.ContainsKey(eventType))
				_subscribers[eventType] = new List<object>();

			_subscribers[eventType].Add(action);
		}

		private void RemoveSubscriber(Type eventType, object action)
		{
			if (!_subscribers.ContainsKey(eventType))
				return;

			_subscribers[eventType].Remove(action);

			if (_subscribers[eventType].Count == 0)
				_subscribers.Remove(eventType);
		}
	}
}