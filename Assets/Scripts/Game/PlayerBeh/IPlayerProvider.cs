using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Data.Config;
using UnityEngine;

namespace ROC.Game.PlayerBeh
{
	public interface IPlayerProvider : IDisposable
	{
		UniTask<Player> CreatePlayer(Vector3 position, CancellationToken cancellationToken);
		UniTask<PlayerConfig> GetPlayerConfig(CancellationToken cancellationToken);
		UniTask DestroyPlayer(CancellationToken cancellationToken);
		Player CurrentPlayer { get; }
	}
}