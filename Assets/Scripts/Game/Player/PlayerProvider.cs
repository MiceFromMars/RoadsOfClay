using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Data.Config;
using UnityEngine;
using VContainer;

namespace ROC.Game.Player
{
	public class PlayerProvider : IPlayerProvider
	{
		private readonly IAssetsProvider _assetsProvider;
		private readonly ILoggingService _logger;

		private PlayerConfig _playerConfig;
		private GameObject _playerInstance;

		public PlayerBehavior CurrentPlayer { get; private set; }

		[Inject]
		public PlayerProvider(
			IAssetsProvider assetsProvider,
			ILoggingService logger)
		{
			_assetsProvider = assetsProvider;
			_logger = logger;
		}

		public async UniTask<PlayerBehavior> CreatePlayer(Vector3 position, CancellationToken cancellationToken)
		{
			if (CurrentPlayer != null)
			{
				_logger.LogWarning("Player already exists. Destroying the existing one before creating a new instance.");
				await DestroyPlayer(cancellationToken);
			}

			try
			{
				// Ensure we have the config loaded
				await GetPlayerConfig(cancellationToken);

				// Instantiate the player prefab
				_playerInstance = await _assetsProvider.InstantiateAsync(AssetsKeys.Player);

				if (_playerInstance == null)
				{
					_logger.LogError($"Failed to instantiate player prefab: {AssetsKeys.Player}");
					return null;
				}

				// Position the player
				_playerInstance.transform.position = position;

				// Get and initialize the player behavior
				CurrentPlayer = _playerInstance.GetComponent<PlayerBehavior>();

				if (CurrentPlayer == null)
				{
					_logger.LogError("Player prefab does not have a PlayerBehavior component");
					GameObject.Destroy(_playerInstance);
					_playerInstance = null;
					return null;
				}

				// Initialize player with config
				CurrentPlayer.Initialize(_playerConfig);

				return CurrentPlayer;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error creating player: {ex.Message}");

				if (_playerInstance != null)
				{
					GameObject.Destroy(_playerInstance);
					_playerInstance = null;
				}

				CurrentPlayer = null;
				return null;
			}
		}

		public async UniTask<PlayerConfig> GetPlayerConfig(CancellationToken cancellationToken)
		{
			if (_playerConfig != null)
				return _playerConfig;

			_playerConfig = await _assetsProvider.LoadAssetAsync<PlayerConfig>(AssetsKeys.PlayerConfig);

			if (_playerConfig == null)
				_logger.LogError($"Failed to load player config: {AssetsKeys.PlayerConfig}");

			return _playerConfig;
		}

		public async UniTask DestroyPlayer(CancellationToken cancellationToken)
		{
			if (_playerInstance == null)
				return;

			try
			{
				// Clean up player behavior
				if (CurrentPlayer != null)
				{
					CurrentPlayer.Cleanup();
					CurrentPlayer = null;
				}

				// Destroy the GameObject
				GameObject.Destroy(_playerInstance);
				_playerInstance = null;

				await UniTask.CompletedTask;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error destroying player: {ex.Message}");
			}
		}

		public void Dispose()
		{
			if (_playerConfig != null)
			{
				_assetsProvider.Release(_playerConfig);
				_playerConfig = null;
			}

			if (_playerInstance != null)
			{
				GameObject.Destroy(_playerInstance);
				_playerInstance = null;
				CurrentPlayer = null;
			}
		}
	}
}