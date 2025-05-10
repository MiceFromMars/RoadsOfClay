using ROC.Core.Events;
using ROC.Data.Config;

namespace ROC.Game.Levels
{
	public readonly struct LevelLoadedEvent : IEvent
	{
		public readonly int LevelIndex;
		public readonly LevelConfig LevelConfig;

		public LevelLoadedEvent(int levelIndex, LevelConfig levelConfig)
		{
			LevelIndex = levelIndex;
			LevelConfig = levelConfig;
		}
	}

	public readonly struct LevelUnloadingEvent : IEvent
	{
		public readonly int LevelIndex;

		public LevelUnloadingEvent(int levelIndex)
		{
			LevelIndex = levelIndex;
		}
	}
}