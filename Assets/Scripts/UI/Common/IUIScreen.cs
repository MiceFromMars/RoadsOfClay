using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ROC.UI.Common
{
	public interface IUIScreen : IDisposable
	{
		/// <summary>
		/// The GameObject this screen is attached to.
		/// </summary>
		GameObject gameObject { get; }

		/// <summary>
		/// Shows the UI screen with animation.
		/// </summary>
		UniTask Show(CancellationToken cancellationToken = default);

		/// <summary>
		/// Hides the UI screen with animation.
		/// </summary>
		UniTask Hide(CancellationToken cancellationToken = default);
	}
}