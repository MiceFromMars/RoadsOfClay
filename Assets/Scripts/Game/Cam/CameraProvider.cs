using System;
using System.Threading;
using Unity.Cinemachine;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Data.Config;
using UnityEngine;

namespace ROC.Game.Cam
{
	public class CameraProvider : ICameraProvider
	{
		private readonly IAssetsProvider _assetsProvider;
		private readonly ILoggingService _logger;
		private CameraConfig _cameraConfig;
		public CinemachineCamera CurrentCamera { get; private set; }

		public CameraProvider(
			IAssetsProvider assetsProvider,
			ILoggingService logger)
		{
			_assetsProvider = assetsProvider;
			_logger = logger;
		}

		public async UniTask<CinemachineCamera> CreateCamera(CancellationToken cancellationToken)
		{
			if (CurrentCamera != null)
			{
				_logger.LogWarning("Camera already exists. Destroying the existing one before creating a new instance.");
				await DestroyCamera(cancellationToken);
			}

			try
			{
				// Ensure we have the config loaded
				await GetCameraConfig(cancellationToken);

				// Instantiate the camera prefab
				GameObject cameraInstance = await _assetsProvider.InstantiateAsync(AssetsKeys.Camera);

				if (cameraInstance == null)
				{
					_logger.LogError($"Failed to instantiate camera prefab: {AssetsKeys.Camera}");
					return null;
				}

				// Get the camera components
				CurrentCamera = cameraInstance.GetComponent<CinemachineCamera>();
				if (CurrentCamera == null)
				{
					_logger.LogError("Cinemachine Camera component not found on the instantiated prefab");
					GameObject.Destroy(cameraInstance);
					return null;
				}

				return CurrentCamera;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error creating camera: {ex.Message}");
				CurrentCamera = null;
				return null;
			}
		}

		public void SetTarget(Transform target)
		{
			if (CurrentCamera == null)
			{
				_logger.LogError("Cannot set target: Virtual camera is not initialized");
				return;
			}

			// Set the Cinemachine target to follow and look at
			CurrentCamera.Follow = target;
			CurrentCamera.LookAt = target;
			CurrentCamera.gameObject.SetActive(true);
		}

		public void SetBoundingShape(Collider2D boundingShape)
		{
			if (CurrentCamera == null)
			{
				_logger.LogError("Cannot set bounding shape: Camera is not initialized");
				return;
			}

			if (boundingShape == null)
			{
				_logger.LogError("Cannot set bounding shape: Provided collider is null");
				return;
			}

			// Get or add the CinemachineConfiner2D component
			var confiner = CurrentCamera.GetComponent<CinemachineConfiner2D>();
			if (confiner == null)
			{
				confiner = CurrentCamera.gameObject.AddComponent<CinemachineConfiner2D>();
			}

			// Set the bounding shape
			confiner.BoundingShape2D = boundingShape;

			// Ensure confiner is enabled and set damping
			confiner.enabled = true;
			confiner.Damping = _cameraConfig != null ? _cameraConfig.ConfinerDamping : 0.2f;

			// Call InvalidateCache to update the confiner with the new bounding shape
			confiner.InvalidateCache();
		}

		public async UniTask<CameraConfig> GetCameraConfig(CancellationToken cancellationToken)
		{
			if (_cameraConfig != null)
				return _cameraConfig;

			_cameraConfig = await _assetsProvider.LoadAssetAsync<CameraConfig>(AssetsKeys.CameraConfig);

			if (_cameraConfig == null)
				_logger.LogError($"Failed to load camera config: {AssetsKeys.CameraConfig}");

			return _cameraConfig;
		}

		public async UniTask DestroyCamera(CancellationToken cancellationToken)
		{
			if (CurrentCamera == null)
				return;

			try
			{
				GameObject cameraInstance = null;

				if (CurrentCamera != null)
					cameraInstance = CurrentCamera.gameObject;

				// Clean up references
				CurrentCamera = null;

				// Destroy the GameObject if it exists
				if (cameraInstance != null)
					GameObject.Destroy(cameraInstance);

				await UniTask.CompletedTask;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error destroying camera: {ex.Message}");
			}
		}

		public void Dispose()
		{
			if (_cameraConfig != null)
			{
				_assetsProvider.Release(_cameraConfig);
				_cameraConfig = null;
			}

			// Destroy the camera instance if it exists
			if (CurrentCamera != null)
			{
				GameObject.Destroy(CurrentCamera.gameObject);
				CurrentCamera = null;
			}
		}
	}
}