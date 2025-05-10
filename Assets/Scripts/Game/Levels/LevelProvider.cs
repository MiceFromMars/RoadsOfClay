using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.Data.Config;
using UnityEngine;
using VContainer;

namespace ROC.Game.Levels
{
	public class LevelProvider : ILevelProvider
	{
		private readonly IEventBus _eventBus;
		private readonly ILevelLoader _levelLoader;

		public Level CurrentLevel { get; private set; }
		public int CurrentLevelIndex { get; private set; } = -1;
		public LevelConfig CurrentLevelConfig { get; private set; }

		[Inject]
		public LevelProvider(
			IEventBus eventBus,
			ILevelLoader levelLoader)
		{
			_eventBus = eventBus;
			_levelLoader = levelLoader;
		}

		public async UniTask LoadLevel(int levelIndex, CancellationToken cancellationToken)
		{
			// Load level using the loader
			CurrentLevel = await _levelLoader.LoadLevel(levelIndex, cancellationToken);

			if (CurrentLevel != null)
			{
				CurrentLevelIndex = levelIndex;
				CurrentLevelConfig = _levelLoader.CurrentLevelConfig;

				// Notify level loaded
				_eventBus.Fire(new LevelLoadedEvent(CurrentLevelIndex, CurrentLevelConfig));
			}
		}

		public async UniTask UnloadLevel(CancellationToken cancellationToken)
		{
			if (CurrentLevel == null)
				return;

			// Notify level unloading
			_eventBus.Fire(new LevelUnloadingEvent(CurrentLevelIndex));

			// Delegate to loader
			await _levelLoader.UnloadLevel(cancellationToken);

			CurrentLevel = null;
			CurrentLevelIndex = -1;
			CurrentLevelConfig = null;
		}

		public void Dispose()
		{
			// No need to dispose anything here as _levelLoader is managed by DI container
		}
	}
}