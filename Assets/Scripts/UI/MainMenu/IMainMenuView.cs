using ROC.UI.Common;
using System;

namespace ROC.UI.MainMenu
{
	public interface IMainMenuView : IView
	{
		void SetPlayButtonClickListener(Action callback);
		void SetLevelSelectionButtonClickListener(Action callback);
		//void SetQuitButtonClickListener(Action callback);
	}
}