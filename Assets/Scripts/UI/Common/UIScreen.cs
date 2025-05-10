using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ROC.UI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.Common
{
	public abstract class UIScreen : MonoBehaviour, IUIScreen
	{
		[SerializeField] private CanvasGroup _canvasGroup;
		[SerializeField] private float _fadeInDuration = 0.3f;
		[SerializeField] private float _fadeOutDuration = 0.2f;

		protected CancellationTokenSource _cts = new CancellationTokenSource();
		protected Sequence _currentAnimation;

		protected virtual void Awake()
		{
			if (_canvasGroup == null)
				_canvasGroup = GetComponent<CanvasGroup>();

			if (_canvasGroup == null)
				_canvasGroup = gameObject.AddComponent<CanvasGroup>();

			_canvasGroup.alpha = 0f;
			_canvasGroup.SetInteractable(false);
		}

		protected virtual void OnDestroy()
		{
			_currentAnimation?.Kill();
		}

		public virtual async UniTask Show(CancellationToken cancellationToken = default)
		{
			gameObject.SetActive(true);
			_canvasGroup.SetInteractable(true);

			_currentAnimation?.Kill();
			await _canvasGroup.FadeToAsync(1f, _fadeInDuration, Ease.OutQuad, cancellationToken);
		}

		public virtual async UniTask Hide(CancellationToken cancellationToken = default)
		{
			_canvasGroup.SetInteractable(false);

			_currentAnimation?.Kill();
			await _canvasGroup.FadeToAsync(0f, _fadeOutDuration, Ease.InQuad, cancellationToken);

			gameObject.SetActive(false);
		}

		public virtual void Dispose()
		{
			_currentAnimation?.Kill();
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = null;
		}
	}
}