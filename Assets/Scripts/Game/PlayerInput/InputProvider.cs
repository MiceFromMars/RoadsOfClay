using UnityEngine;
using VContainer.Unity;

namespace ROC.Game.PlayerInput
{
	public class InputProvider : IInputProvider, ITickable
	{
		private bool _isLeftPressed;
		private bool _isRightPressed;
		private bool _isJumpPressed;

		public bool IsLeftPressed => _isLeftPressed;
		public bool IsRightPressed => _isRightPressed;
		public bool IsJumpPressed => _isJumpPressed;

		public void Tick()
		{
			ProcessKeyboardInput();
		}

		private void ProcessKeyboardInput()
		{
			// Left input
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				SetLeftPressed(true);
			}
			if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
			{
				SetLeftPressed(false);
			}

			// Right input
			if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				SetRightPressed(true);
			}
			if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
			{
				SetRightPressed(false);
			}

			// Jump input
			if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
			{
				SetJumpPressed(true);
			}
			if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.Space))
			{
				SetJumpPressed(false);
			}
		}

		private void SetLeftPressed(bool isPressed)
		{
			_isLeftPressed = isPressed;
		}

		private void SetRightPressed(bool isPressed)
		{
			_isRightPressed = isPressed;
		}

		private void SetJumpPressed(bool isPressed)
		{
			_isJumpPressed = isPressed;
		}

		// Method to handle HUD button input
		public void HandleHUDInput(InputType inputType, bool isPressed)
		{
			switch (inputType)
			{
				case InputType.Left:
					SetLeftPressed(isPressed);
					break;
				case InputType.Right:
					SetRightPressed(isPressed);
					break;
				case InputType.Jump:
					SetJumpPressed(isPressed);
					break;
			}
		}
	}
}