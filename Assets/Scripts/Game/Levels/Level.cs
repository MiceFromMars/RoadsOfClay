using UnityEngine;
using ROC.Core.Assets;

namespace ROC.Game.Levels
{
	public class Level : MonoBehaviour
	{
		[SerializeField] private Transform _playerSpawnPoint;
		[SerializeField] private Transform _leftBound;
		[SerializeField] private Transform _rightBound;
		[SerializeField] private Transform _upBound;
		[SerializeField] private Transform _downBound;

		public Vector3 GetPlayerSpawnPoint()
		{
			if (_playerSpawnPoint == null)
			{
				Debug.LogWarning($"[{AssetsKeys.LevelPrefabPath}] Player spawn point is missing on {gameObject.name}. Using level transform position as fallback.");
				return transform.position;
			}
			return _playerSpawnPoint.position;
		}

		public Bounds GetLevelBounds()
		{
			if (_leftBound == null || _rightBound == null || _upBound == null || _downBound == null)
			{
				// Return default bounds if any bound is missing
				Debug.LogWarning($"[{AssetsKeys.LevelPrefabPath}] Level bounds are incomplete on {gameObject.name}. Using default bounds.");
				return new Bounds(transform.position, Vector3.one * 10f);
			}

			// Calculate center
			Vector3 center = new Vector3(
				(_leftBound.position.x + _rightBound.position.x) * 0.5f,
				(_downBound.position.y + _upBound.position.y) * 0.5f,
				0f);

			// Calculate size
			Vector3 size = new Vector3(
				Mathf.Abs(_rightBound.position.x - _leftBound.position.x),
				Mathf.Abs(_upBound.position.y - _downBound.position.y),
				10f); // Using a default Z size

			return new Bounds(center, size);
		}
	}
}