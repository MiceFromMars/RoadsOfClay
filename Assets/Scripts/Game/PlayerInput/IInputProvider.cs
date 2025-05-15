namespace ROC.Game.PlayerInput
{
	public interface IInputProvider
	{
		bool IsLeftPressed { get; }
		bool IsRightPressed { get; }
		bool IsJumpPressed { get; }

		void HandleHUDInput(InputType inputType, bool isPressed);
	}
}