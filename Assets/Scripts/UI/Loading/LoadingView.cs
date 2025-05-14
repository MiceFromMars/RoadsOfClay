using ROC.UI.Common;
using UnityEngine;

namespace ROC.UI.Loading
{
	public class LoadingView : BaseView, ILoadingView
	{
		public GameObject GameObject => gameObject;

		protected override void InitializeView()
		{
			base.InitializeView();
			Layer = UILayer.Loading;
		}
	}
}