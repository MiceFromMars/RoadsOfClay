using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Data.Config;
using UnityEngine;

namespace ROC.Game.Levels
{
	public interface ILevelLoader : IDisposable
	{
		UniTask<Level> LoadLevel(int levelIndex, CancellationToken cancellationToken);
		UniTask UnloadLevel(CancellationToken cancellationToken);
		UniTask CleanupLevel(CancellationToken cancellationToken);
		Level CurrentLevel { get; }
		LevelConfig CurrentLevelConfig { get; }
		int CurrentLevelIndex { get; }
	}
}