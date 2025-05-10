using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.Utils
{
	public static class UIExtensions
	{
		/// <summary>
		/// Fades a CanvasGroup over time.
		/// </summary>
		public static async UniTask FadeToAsync(this CanvasGroup canvasGroup, float targetAlpha, float duration,
			Ease ease = Ease.Linear, CancellationToken cancellationToken = default)
		{
			await canvasGroup.DOFade(targetAlpha, duration)
				.SetEase(ease)
				.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Enables or disables a CanvasGroup's interactivity.
		/// </summary>
		public static void SetInteractable(this CanvasGroup canvasGroup, bool interactable)
		{
			canvasGroup.interactable = interactable;
			canvasGroup.blocksRaycasts = interactable;
		}

		/// <summary>
		/// Scales a RectTransform over time.
		/// </summary>
		public static async UniTask ScaleToAsync(this RectTransform rectTransform, Vector3 targetScale, float duration,
			Ease ease = Ease.OutBack, CancellationToken cancellationToken = default)
		{
			await rectTransform.DOScale(targetScale, duration)
				.SetEase(ease)
				.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Moves a RectTransform to a target anchored position over time.
		/// </summary>
		public static async UniTask MoveToAsync(this RectTransform rectTransform, Vector2 targetPosition, float duration,
			Ease ease = Ease.OutQuad, CancellationToken cancellationToken = default)
		{
			await rectTransform.DOAnchorPos(targetPosition, duration)
				.SetEase(ease)
				.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Changes the color of an Image over time.
		/// </summary>
		public static async UniTask ColorToAsync(this Image image, Color targetColor, float duration,
			Ease ease = Ease.Linear, CancellationToken cancellationToken = default)
		{
			await image.DOColor(targetColor, duration)
				.SetEase(ease)
				.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Shakes a RectTransform.
		/// </summary>
		public static async UniTask ShakeAsync(this RectTransform rectTransform, float duration = 0.5f, float strength = 10f,
			int vibrato = 10, CancellationToken cancellationToken = default)
		{
			await rectTransform.DOShakePosition(duration, strength, vibrato)
				.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Pulses a RectTransform by scaling it up and then back to its original scale.
		/// </summary>
		public static async UniTask PulseAsync(this RectTransform rectTransform, float scaleFactor = 1.1f, float duration = 0.3f,
			CancellationToken cancellationToken = default)
		{
			Vector3 originalScale = rectTransform.localScale;
			Vector3 targetScale = originalScale * scaleFactor;

			Sequence sequence = DOTween.Sequence();
			sequence.Append(rectTransform.DOScale(targetScale, duration * 0.5f).SetEase(Ease.OutQuad));
			sequence.Append(rectTransform.DOScale(originalScale, duration * 0.5f).SetEase(Ease.InQuad));

			await sequence.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Extension method to convert a Tween to a UniTask with cancellation support.
		/// </summary>
		public static UniTask ToUniTask(this Tween tween, CancellationToken cancellationToken = default)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				tween.Kill();
				return UniTask.FromCanceled(cancellationToken);
			}

			var source = new UniTaskCompletionSource();

			tween.OnComplete(() => source.TrySetResult());
			tween.OnKill(() => source.TrySetCanceled());

			cancellationToken.Register(() =>
			{
				if (tween.IsActive())
				{
					tween.Kill();
					source.TrySetCanceled();
				}
			});

			return source.Task;
		}
	}
}