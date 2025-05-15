using UnityEngine;

namespace ROC.Data.Config
{
	[CreateAssetMenu(fileName = "PlayerConfig", menuName = "ROC/Config/PlayerConfig")]
	public class PlayerConfig : ScriptableObject
	{
		[SerializeField] private int _startLives = 3;
		[SerializeField] private float _moveSpeed = 300f;
		[SerializeField] private float _jumpForce = 50f;
		[SerializeField] private float _jumpCooldown = 0.25f;

		public int StartLives => _startLives;
		public float MoveSpeed => _moveSpeed;
		public float JumpForce => _jumpForce;
		public float JumpCooldown => _jumpCooldown;
	}
}