using ROC.UI.Common;
using System;

namespace ROC.UI.GameOver
{
	public interface IGameOverView : IView
	{
		void SetResult(bool isWin);
		void SetScore(int score);
		void SetLevel(int level);
		void SetRestartButtonListener(Action callback);
		void SetNextLevelButtonListener(Action callback);
		void SetMainMenuButtonListener(Action callback);
	}
}