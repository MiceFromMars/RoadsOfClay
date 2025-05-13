using ROC.UI.Common;
using System;

namespace ROC.UI.HUD
{
	public interface IGameHUDView : IView
	{
		void SetLeftButtonListeners(Action onPress, Action onRelease);
		void SetRightButtonListeners(Action onPress, Action onRelease);
		void SetJumpButtonListeners(Action onPress, Action onRelease);
		void UpdateLives(int current, int max);
		void UpdateScore(int score);
		void UpdateHeight(float height);
		void UpdateSpeed(float speed);
	}
}