using System;

namespace ROC.Core.Events
{
	public interface IEventBus
	{
		void Subscribe<TEvent>(Action<TEvent> action) where TEvent : struct, IEvent;
		void Unsubscribe<TEvent>(Action<TEvent> action) where TEvent : struct, IEvent;
		void Fire<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
	}
}