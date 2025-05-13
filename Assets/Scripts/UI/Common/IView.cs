using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ROC.UI.Common
{
	public interface IView
	{
		GameObject GameObject { get; }
		UILayer Layer { get; set; }
		UniTask Hide(CancellationToken cancellationToken);
		UniTask Show(CancellationToken cancellationToken);
		void Dispose();
	}
}