using UnityEngine;

namespace ROC.Data.Config
{
	[CreateAssetMenu(fileName = "CameraConfig", menuName = "ROC/Config/CameraConfig")]
	public class CameraConfig : ScriptableObject
	{
		[SerializeField] private float _followSpeed = 2.0f;
		[SerializeField] private float _lookAheadDistance = 2.0f;
		[SerializeField] private float _verticalOffset = 1.0f;
		[SerializeField] private float _followDamping = 0.1f;

		public float FollowSpeed => _followSpeed;
		public float LookAheadDistance => _lookAheadDistance;
		public float VerticalOffset => _verticalOffset;
		public float FollowDamping => _followDamping;
	}
}