using UnityEngine;
using ROC.Game.PlayerInput;
using ROC.Data.Config;
using VContainer;

namespace ROC.Game.PlayerBeh
{
	public class PlayerMove : MonoBehaviour
	{
		private IInputProvider _inputProvider;

		private Rigidbody2D _rb;
		private float _speed;

		[Inject]
		public void Initialize(IInputProvider inputProvider, PlayerConfig config)
		{
			_inputProvider = inputProvider;

			_rb = GetComponent<Rigidbody2D>();
			_speed = config.MoveSpeed;
		}

		private void FixedUpdate()
		{
			Move();
		}

		private void Move()
		{
			if (_inputProvider.IsLeftPressed)
			{
				_rb.AddTorque(_speed * Time.fixedDeltaTime);
			}

			if (_inputProvider.IsRightPressed)
			{
				_rb.AddTorque(_speed * -1 * Time.fixedDeltaTime);
			}
		}
	}
}