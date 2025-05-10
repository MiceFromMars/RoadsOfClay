using ROC.Data.Config;
using UnityEngine;

namespace ROC.Game.Player
{
	public class CameraFollow : MonoBehaviour
	{
		private Transform _target;
		private CameraConfig _config;
		private Vector3 _velocity = Vector3.zero;
		private bool _isInitialized;

		public void Initialize(Transform target, CameraConfig config)
		{
			_target = target;
			_config = config;
			_isInitialized = true;
		}

		public void Cleanup()
		{
			_target = null;
			_config = null;
			_isInitialized = false;
		}

		private void LateUpdate()
		{
			if (!_isInitialized || _target == null || _config == null)
				return;

			// Calculate target position
			Vector3 targetPosition = _target.position;

			// Apply vertical offset
			targetPosition.y += _config.VerticalOffset;

			// Apply look ahead distance based on target's facing direction
			// Assuming the target forward vector defines the facing direction
			targetPosition += _target.forward * _config.LookAheadDistance;

			// Keep the camera's Z position unchanged
			targetPosition.z = transform.position.z;

			// Smoothly move towards the target position
			transform.position = Vector3.SmoothDamp(
				transform.position,
				targetPosition,
				ref _velocity,
				_config.FollowDamping,
				_config.FollowSpeed);
		}
	}
}