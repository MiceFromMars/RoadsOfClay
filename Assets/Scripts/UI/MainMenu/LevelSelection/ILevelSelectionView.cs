using ROC.UI.Common;
using System;

namespace ROC.UI.MainMenu.LevelSelection
{
	public interface ILevelSelectionView : IView
	{
		void SetBackButtonClickListener(Action callback);
		void SetLevelButtonsState(bool[] unlockedLevels);
		void SetLevelSelectedListener(Action<int> callback);
	}
}