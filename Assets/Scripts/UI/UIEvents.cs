using ROC.Core.Events;

namespace ROC.UI
{
	public struct MainMenuOpenedEvent : IEvent { }

	public struct LevelSelectionOpenedEvent : IEvent { }

	public struct GameScreenOpenedEvent : IEvent { }

	public struct GameOverScreenOpenedEvent : IEvent
	{
		public bool IsWin;
		public int LevelIndex;
		public int Score;
	}

	public struct ReturnToMainMenuEvent : IEvent { }

	public struct RestartLevelEvent : IEvent
	{
		public int LevelIndex;
	}

	public struct NextLevelEvent : IEvent
	{
		public int LevelIndex;
	}

	public struct LevelSelectedEvent : IEvent
	{
		public int LevelIndex;
	}

	public struct PlayerLivesChangedEvent : IEvent
	{
		public int CurrentLives;
		public int MaxLives;
	}

	public struct ScoreChangedEvent : IEvent
	{
		public int Score;
	}

	public struct HeightChangedEvent : IEvent
	{
		public float Height;
	}

	public struct SpeedChangedEvent : IEvent
	{
		public float Speed;
	}
}