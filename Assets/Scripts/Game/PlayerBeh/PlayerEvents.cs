using ROC.Core.Events;

namespace ROC.Game.PlayerBeh
{
	public readonly struct PlayerDamagedEvent : IEvent
	{
		public readonly int RemainingLives { get; }

		public PlayerDamagedEvent(int remainingLives)
		{
			RemainingLives = remainingLives;
		}
	}

	public readonly struct PlayerDiedEvent : IEvent
	{
		public readonly int FinalScore { get; }
		public readonly float MaxHeight { get; }
		public readonly float MaxSpeed { get; }

		public PlayerDiedEvent(int finalScore, float maxHeight, float maxSpeed)
		{
			FinalScore = finalScore;
			MaxHeight = maxHeight;
			MaxSpeed = maxSpeed;
		}
	}

	public readonly struct ScoreChangedEvent : IEvent
	{
		public readonly int NewScore { get; }

		public ScoreChangedEvent(int newScore)
		{
			NewScore = newScore;
		}
	}

	public readonly struct MaxHeightChangedEvent : IEvent
	{
		public readonly float NewMaxHeight { get; }

		public MaxHeightChangedEvent(float newMaxHeight)
		{
			NewMaxHeight = newMaxHeight;
		}
	}

	public readonly struct MaxSpeedChangedEvent : IEvent
	{
		public readonly float NewMaxSpeed { get; }

		public MaxSpeedChangedEvent(float newMaxSpeed)
		{
			NewMaxSpeed = newMaxSpeed;
		}
	}
}