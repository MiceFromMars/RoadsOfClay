using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Data.Config;
using UnityEngine;

namespace ROC.Game.Player
{
	public interface IPlayerProvider : IDisposable
	{
		UniTask<PlayerBehavior> CreatePlayer(Vector3 position, CancellationToken cancellationToken);
		UniTask<PlayerConfig> GetPlayerConfig(CancellationToken cancellationToken);
		UniTask DestroyPlayer(CancellationToken cancellationToken);
		PlayerBehavior CurrentPlayer { get; }
	}
}