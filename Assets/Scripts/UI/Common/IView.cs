using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ROC.UI.Common
{
	/// <summary>
	/// Base interface for all View components in the MVP pattern
	/// </summary>
	public interface IView : IDisposable
	{
		/// <summary>
		/// The GameObject this view is attached to
		/// </summary>
		GameObject gameObject { get; }

		/// <summary>
		/// Shows the view with animation
		/// </summary>
		UniTask Show(CancellationToken cancellationToken = default);

		/// <summary>
		/// Hides the view with animation
		/// </summary>
		UniTask Hide(CancellationToken cancellationToken = default);
	}
}