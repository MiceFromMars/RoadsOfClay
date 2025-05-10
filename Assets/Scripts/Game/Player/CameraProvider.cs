using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Data.Config;
using UnityEngine;
using VContainer;

namespace ROC.Game.Player
{
	public class CameraProvider : ICameraProvider
	{
		private readonly IAssetsProvider _assetsProvider;
		private readonly ILoggingService _logger;

		private CameraConfig _cameraConfig;
		private GameObject _cameraInstance;

		public Camera CurrentCamera { get; private set; }

		[Inject]
		public CameraProvider(
			IAssetsProvider assetsProvider,
			ILoggingService logger)
		{
			_assetsProvider = assetsProvider;
			_logger = logger;
		}

		public async UniTask<Camera> CreateCamera(Transform target, CancellationToken cancellationToken)
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
				_cameraInstance = await _assetsProvider.InstantiateAsync(AssetsKeys.Camera);

				if (_cameraInstance == null)
				{
					_logger.LogError($"Failed to instantiate camera prefab: {AssetsKeys.Camera}");
					return null;
				}

				// Get the camera component
				CurrentCamera = _cameraInstance.GetComponent<Camera>();

				if (CurrentCamera == null)
				{
					_logger.LogError("Camera prefab does not have a Camera component");
					GameObject.Destroy(_cameraInstance);
					_cameraInstance = null;
					return null;
				}

				// Set up camera follow component if there is one
				CameraFollow cameraFollow = _cameraInstance.GetComponent<CameraFollow>();
				if (cameraFollow != null && target != null)
				{
					cameraFollow.Initialize(target, _cameraConfig);
				}

				return CurrentCamera;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error creating camera: {ex.Message}");

				if (_cameraInstance != null)
				{
					GameObject.Destroy(_cameraInstance);
					_cameraInstance = null;
				}

				CurrentCamera = null;
				return null;
			}
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
			if (_cameraInstance == null)
				return;

			try
			{
				// Clean up camera follow component if there is one
				CameraFollow cameraFollow = _cameraInstance.GetComponent<CameraFollow>();
				if (cameraFollow != null)
				{
					cameraFollow.Cleanup();
				}

				CurrentCamera = null;

				// Destroy the GameObject
				GameObject.Destroy(_cameraInstance);
				_cameraInstance = null;

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

			if (_cameraInstance != null)
			{
				GameObject.Destroy(_cameraInstance);
				_cameraInstance = null;
				CurrentCamera = null;
			}
		}
	}
}