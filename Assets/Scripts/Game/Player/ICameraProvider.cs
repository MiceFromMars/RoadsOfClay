using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Data.Config;
using UnityEngine;

namespace ROC.Game.Player
{
	public interface ICameraProvider : IDisposable
	{
		UniTask<Camera> CreateCamera(Transform target, CancellationToken cancellationToken);
		UniTask<CameraConfig> GetCameraConfig(CancellationToken cancellationToken);
		UniTask DestroyCamera(CancellationToken cancellationToken);
		Camera CurrentCamera { get; }
	}
}