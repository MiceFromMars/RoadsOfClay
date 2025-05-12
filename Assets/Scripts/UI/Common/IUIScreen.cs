using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.UI;

namespace ROC.UI.Common
{
	public interface IUIScreen : IView
	{
		UILayer Layer { get; set; }
		bool IsVisible { get; }
		event Action<IUIScreen> OnScreenShown;
		event Action<IUIScreen> OnScreenHidden;
	}
}
