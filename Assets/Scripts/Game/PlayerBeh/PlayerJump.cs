using UnityEngine;
using ROC.Game.PlayerInput;
using VContainer;
using ROC.Data.Config;

namespace ROC.Game.PlayerBeh
{
	public class PlayerJump : MonoBehaviour
	{
		private IInputProvider _inputProvider;

		private GroundSensor _groundSensor;
		private Rigidbody2D _rb;
		private float _jumpForce;
		private float _jumpCooldown;

		private float _cooldownTimer;

		[Inject]
		public void Initialize(IInputProvider inputProvider, PlayerConfig config)
		{
			_inputProvider = inputProvider;

			_groundSensor = GetComponent<GroundSensor>();
			_rb = GetComponent<Rigidbody2D>();

			_jumpForce = config.JumpForce;
			_jumpCooldown = config.JumpCooldown;
		}

		private void FixedUpdate()
		{
			if (_cooldownTimer > 0)
			{
				_cooldownTimer -= Time.fixedDeltaTime;
				return;
			}

			if (_inputProvider.IsJumpPressed)
			{
				if (_groundSensor.IsGrounded)
				{
					Jump();
				}
			}
		}

		private void Jump()
		{
			_cooldownTimer = _jumpCooldown;
			_rb.AddForceY(_jumpForce, ForceMode2D.Impulse);
		}
	}
}