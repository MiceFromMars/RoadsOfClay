using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Data.Config;
using UnityEngine;

namespace ROC.Game.Levels
{
	public interface ILevelProvider : IDisposable
	{
		UniTask LoadLevel(int levelIndex, CancellationToken cancellationToken);
		UniTask UnloadLevel(CancellationToken cancellationToken);
		Level CurrentLevel { get; }
		int CurrentLevelIndex { get; }
		LevelConfig CurrentLevelConfig { get; }
	}
}