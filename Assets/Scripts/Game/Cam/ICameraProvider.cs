using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Data.Config;
using UnityEngine;
using Unity.Cinemachine;

namespace ROC.Game.Cam
{
	public interface ICameraProvider : IDisposable
	{
		UniTask<CinemachineCamera> CreateCamera(CancellationToken cancellationToken);
		UniTask<CameraConfig> GetCameraConfig(CancellationToken cancellationToken);
		UniTask DestroyCamera(CancellationToken cancellationToken);
		void SetTarget(Transform target);
		void SetBoundingShape(Collider2D boundingShape);
		CinemachineCamera CurrentCamera { get; }
	}
}