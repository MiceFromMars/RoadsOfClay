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
		/// Shows a UI screen of type T using the provided asset reference.
		/// </summary>
		UniTask<T> Show<T>(AssetReference screenReference, CancellationToken cancellationToken = default) where T : IUIScreen;

		/// <summary>
		/// Shows a UI screen of type T using the address ID.
		/// </summary>
		UniTask<T> Show<T>(string screenAddress, CancellationToken cancellationToken = default) where T : IUIScreen;

		/// <summary>
		/// Hides a UI screen of type T.
		/// </summary>
		UniTask Hide<T>(CancellationToken cancellationToken = default) where T : IUIScreen;

		/// <summary>
		/// Gets an active screen of type T.
		/// </summary>
		T GetScreen<T>() where T : IUIScreen;
	}
}