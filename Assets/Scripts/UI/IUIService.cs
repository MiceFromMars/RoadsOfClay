using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.UI.Common;
using UnityEngine.AddressableAssets;

namespace ROC.UI
{
	public interface IUIService : IDisposable
	{
		/// <summary>
		/// Shows a UI window of type T on a specific layer using the address ID for the view.
		/// </summary>
		UniTask<TPresenter> ShowWindow<TPresenter>(string viewAddress, UILayer layer = UILayer.Content, CancellationToken cancellationToken = default)
			where TPresenter : IPresenter;

		/// <summary>
		/// Shows a UI window of type T on a specific layer using the provided asset reference for the view.
		/// </summary>
		UniTask<TPresenter> ShowWindow<TPresenter>(AssetReference viewReference, UILayer layer = UILayer.Content, CancellationToken cancellationToken = default)
			where TPresenter : IPresenter;

		/// <summary>
		/// Hides a UI window of type T.
		/// </summary>
		UniTask HideWindow<TPresenter>(CancellationToken cancellationToken = default)
			where TPresenter : IPresenter;

		/// <summary>
		/// Gets an active window of type T.
		/// </summary>
		TPresenter GetWindow<TPresenter>()
			where TPresenter : IPresenter;

		/// <summary>
		/// Hides all windows on a specific layer.
		/// </summary>
		UniTask HideLayer(UILayer layer, CancellationToken cancellationToken = default);

		/// <summary>
		/// Checks if a window of type T is currently active.
		/// </summary>
		bool IsWindowActive<TPresenter>() where TPresenter : IPresenter;

		/// <summary>
		/// Completely disposes of a window of type T and removes it from the system.
		/// </summary>
		UniTask ReleaseWindow<TPresenter>(CancellationToken cancellationToken = default)
			where TPresenter : IPresenter;

		/// <summary>
		/// Completely disposes of all windows on a specific layer and removes them from the system.
		/// </summary>
		UniTask ReleaseLayer(UILayer layer, CancellationToken cancellationToken = default);
	}
}