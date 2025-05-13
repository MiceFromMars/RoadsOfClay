using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.Utils
{
	public static class UIAnimations
	{
		/// <summary>
		/// Fades a CanvasGroup in with a slide from bottom effect
		/// </summary>
		public static async UniTask SlideInFromBottom(this RectTransform rectTransform, CanvasGroup canvasGroup,
			float duration = 0.5f, float distance = 100f, CancellationToken cancellationToken = default)
		{
			Vector2 targetPosition = rectTransform.anchoredPosition;
			Vector2 startPosition = targetPosition - new Vector2(0, -distance);

			rectTransform.anchoredPosition = startPosition;
			canvasGroup.alpha = 0f;

			Sequence sequence = DOTween.Sequence();
			sequence.Append(rectTransform.DOAnchorPos(targetPosition, duration).SetEase(Ease.OutQuad));
			sequence.Join(canvasGroup.DOFade(1f, duration).SetEase(Ease.OutQuad));

			await sequence.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Fades a CanvasGroup out with a slide to bottom effect
		/// </summary>
		public static async UniTask SlideOutToBottom(this RectTransform rectTransform, CanvasGroup canvasGroup,
			float duration = 0.3f, float distance = 100f, CancellationToken cancellationToken = default)
		{
			Vector2 startPosition = rectTransform.anchoredPosition;
			Vector2 targetPosition = startPosition - new Vector2(0, -distance);

			Sequence sequence = DOTween.Sequence();
			sequence.Append(rectTransform.DOAnchorPos(targetPosition, duration).SetEase(Ease.InQuad));
			sequence.Join(canvasGroup.DOFade(0f, duration).SetEase(Ease.InQuad));

			await sequence.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Shows a popup with scale in and bounce effect
		/// </summary>
		public static async UniTask PopupScale(this RectTransform rectTransform, CanvasGroup canvasGroup,
			float duration = 0.5f, CancellationToken cancellationToken = default)
		{
			rectTransform.localScale = Vector3.zero;
			canvasGroup.alpha = 0f;

			Sequence sequence = DOTween.Sequence();
			sequence.Append(rectTransform.DOScale(1.1f, duration * 0.7f).SetEase(Ease.OutQuad));
			sequence.Join(canvasGroup.DOFade(1f, duration * 0.7f).SetEase(Ease.OutQuad));
			sequence.Append(rectTransform.DOScale(1f, duration * 0.3f).SetEase(Ease.InOutSine));

			await sequence.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Hides a popup with scale out effect
		/// </summary>
		public static async UniTask PopupScaleOut(this RectTransform rectTransform, CanvasGroup canvasGroup,
			float duration = 0.3f, CancellationToken cancellationToken = default)
		{
			Sequence sequence = DOTween.Sequence();
			sequence.Append(rectTransform.DOScale(1.1f, duration * 0.3f).SetEase(Ease.OutQuad));
			sequence.Append(rectTransform.DOScale(0f, duration * 0.7f).SetEase(Ease.InQuad));
			sequence.Join(canvasGroup.DOFade(0f, duration * 0.7f).SetEase(Ease.InQuad));

			await sequence.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Creates a bounce effect for buttons
		/// </summary>
		public static async UniTask ButtonClickBounce(this RectTransform rectTransform, float duration = 0.3f,
			float scaleFactor = 0.9f, CancellationToken cancellationToken = default)
		{
			Vector3 originalScale = rectTransform.localScale;

			Sequence sequence = DOTween.Sequence();
			sequence.Append(rectTransform.DOScale(originalScale * scaleFactor, duration * 0.3f).SetEase(Ease.OutQuad));
			sequence.Append(rectTransform.DOScale(originalScale, duration * 0.7f).SetEase(Ease.OutElastic));

			await sequence.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Creates a fade and slide panel transition
		/// </summary>
		public static async UniTask SlideHorizontal(this RectTransform rectTransform, CanvasGroup canvasGroup,
			bool toRight, float duration = 0.5f, float distance = 100f, CancellationToken cancellationToken = default)
		{
			Vector2 originalPosition = rectTransform.anchoredPosition;
			Vector2 targetPosition = originalPosition + new Vector2(toRight ? distance : -distance, 0);

			Sequence sequence = DOTween.Sequence();
			sequence.Append(rectTransform.DOAnchorPos(targetPosition, duration).SetEase(Ease.InOutQuad));
			sequence.Join(canvasGroup.DOFade(toRight ? 0f : 1f, duration).SetEase(Ease.InOutQuad));

			await sequence.ToUniTask(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Performs typewriter effect on a TextMeshProUGUI component
		/// </summary>
		public static async UniTask TypewriterEffect(this TMP_Text text, string finalText, float typingSpeed = 0.05f,
			CancellationToken cancellationToken = default)
		{
			string originalText = finalText;
			text.text = string.Empty;

			for (int i = 0; i < originalText.Length; i++)
			{
				if (cancellationToken.IsCancellationRequested)
					break;

				text.text += originalText[i];
				await UniTask.Delay(TimeSpan.FromSeconds(typingSpeed), cancellationToken: cancellationToken);
			}
		}

		/// <summary>
		/// Moves a RectTransform in an arc path from its current position to a target position
		/// </summary>
		public static async UniTask MoveInArc(this RectTransform rectTransform, Vector2 targetPosition, float arcHeight,
			float duration = 0.5f, Ease ease = Ease.InOutSine, CancellationToken cancellationToken = default)
		{
			Vector2 startPos = rectTransform.anchoredPosition;

			// Calculate a control point for the arc
			Vector2 midPoint = (startPos + targetPosition) * 0.5f;

			// Add a perpendicular offset for the control point
			Vector2 direction = (targetPosition - startPos).normalized;
			Vector2 perpendicular = new Vector2(-direction.y, direction.x);
			Vector2 controlPoint = midPoint + perpendicular * arcHeight;

			// Create a path with the control point
			Vector3[] path = new Vector3[3];
			path[0] = startPos;
			path[1] = controlPoint;
			path[2] = targetPosition;

			// Execute the movement with DOTween
			Tween tween = rectTransform.DOPath(path, duration, PathType.CubicBezier)
				.SetEase(ease)
				.SetOptions(true);

			await tween.ToUniTask(cancellationToken: cancellationToken);
		}
	}
}