using System;
using System.Collections.Generic;

namespace ROC.Core.Events
{
	public class EventBus : IEventBus
	{
		private readonly Dictionary<Type, List<object>> _subscribers = new Dictionary<Type, List<object>>();

		public void Subscribe<TEvent>(Action<TEvent> action) where TEvent : struct, IEvent
		{
			Type eventType = typeof(TEvent);

			if (!_subscribers.ContainsKey(eventType))
				_subscribers[eventType] = new List<object>();

			_subscribers[eventType].Add(action);
		}

		public void Unsubscribe<TEvent>(Action<TEvent> action) where TEvent : struct, IEvent
		{
			Type eventType = typeof(TEvent);

			if (!_subscribers.ContainsKey(eventType))
				return;

			_subscribers[eventType].Remove(action);

			if (_subscribers[eventType].Count == 0)
				_subscribers.Remove(eventType);
		}

		public void Fire<TEvent>(TEvent eventData) where TEvent : struct, IEvent
		{
			Type eventType = typeof(TEvent);

			if (!_subscribers.ContainsKey(eventType))
				return;

			foreach (var subscriber in _subscribers[eventType])
			{
				if (subscriber is Action<TEvent> action)
					action.Invoke(eventData);
			}
		}
	}
}