using ROC.Core.Events;

namespace ROC.Game.Enemy
{
	public readonly struct EnemyKilledEvent : IEvent
	{
		public readonly int Points { get; }

		public EnemyKilledEvent(int points)
		{
			Points = points;
		}
	}
}
