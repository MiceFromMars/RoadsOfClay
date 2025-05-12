using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ROC.UI.Common
{
	/// <summary>
	/// Base interface for all Presenter components in the MVP pattern
	/// </summary>
	public interface IPresenter : IDisposable
	{
		/// <summary>
		/// Initialize the presenter and bind to the view
		/// </summary>
		void Initialize();

		/// <summary>
		/// Show the associated view
		/// </summary>
		UniTask Show(CancellationToken cancellationToken = default);

		/// <summary>
		/// Hide the associated view
		/// </summary>
		UniTask Hide(CancellationToken cancellationToken = default);
	}
}