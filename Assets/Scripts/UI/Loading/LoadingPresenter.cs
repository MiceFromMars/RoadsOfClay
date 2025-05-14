using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.UI.Common;

namespace ROC.UI.Loading
{
	public class LoadingPresenter : BasePresenter<ILoadingView>
	{
		private bool _isActive;

		public LoadingPresenter(
			ILoadingView view,
			IEventBus eventBus) : base(view, eventBus)
		{
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			if (_isActive)
				return;

			_isActive = true;
			await base.Show(cancellationToken);
		}

		public override async UniTask Hide(CancellationToken cancellationToken = default)
		{
			if (!_isActive)
				return;

			_isActive = false;
			await base.Hide(cancellationToken);
		}
	}
}