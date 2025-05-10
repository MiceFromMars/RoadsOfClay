using UnityEngine;

namespace ROC.Data.Config
{
	[CreateAssetMenu(fileName = "PlayerConfig", menuName = "ROC/Config/PlayerConfig")]
	public class PlayerConfig : ScriptableObject
	{
		[SerializeField] private float _moveSpeed = 5f;
		[SerializeField] private float _jumpForce = 10f;
		[SerializeField] private int _startLives = 3;

		public float MoveSpeed => _moveSpeed;
		public float JumpForce => _jumpForce;
		public int StartLives => _startLives;
	}
}