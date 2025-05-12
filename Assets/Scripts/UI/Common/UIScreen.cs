using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ROC.UI.Utils;
using UnityEngine;
using UnityEngine.UI;
using ROC.UI;

namespace ROC.UI.Common
{
	public enum ScreenTransitionType
	{
		Fade,
		SlideFromBottom,
		SlideFromTop,
		SlideFromLeft,
		SlideFromRight,
		Scale,
		Custom
	}

	[RequireComponent(typeof(CanvasGroup))]
	public class UIScreen : BaseView, IUIScreen
	{
		[Header("Transition Settings")]
		[SerializeField] private ScreenTransitionType _showTransition = ScreenTransitionType.Fade;
		[SerializeField] private ScreenTransitionType _hideTransition = ScreenTransitionType.Fade;
		[SerializeField] private float _showDuration = 0.3f;
		[SerializeField] private float _hideDuration = 0.2f;
		[SerializeField] private Ease _showEase = Ease.OutQuad;
		[SerializeField] private Ease _hideEase = Ease.InQuad;
		[SerializeField] private float _transitionDistance = 100f;

		public UILayer Layer { get; set; } = UILayer.Content;
		public bool IsVisible { get; private set; }
		public event Action<IUIScreen> OnScreenShown;
		public event Action<IUIScreen> OnScreenHidden;

		protected RectTransform _rectTransform;
		protected new CanvasGroup _canvasGroup;

		protected override void Awake()
		{
			base.Awake();
			_canvasGroup = GetComponent<CanvasGroup>();
			_rectTransform = transform as RectTransform;
			if (_rectTransform == null)
			{
				Debug.LogError($"UIScreen {name} must be attached to a GameObject with a RectTransform component");
				gameObject.AddComponent<RectTransform>();
				_rectTransform = transform as RectTransform;
			}
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			if (IsVisible)
				return;

			gameObject.SetActive(true);

			await ExecuteShowTransition(cancellationToken);

			_canvasGroup.SetInteractable(true);
			IsVisible = true;
			OnScreenShown?.Invoke(this);
		}

		public override async UniTask Hide(CancellationToken cancellationToken = default)
		{
			if (!IsVisible)
				return;

			_canvasGroup.SetInteractable(false);

			await ExecuteHideTransition(cancellationToken);

			IsVisible = false;
			gameObject.SetActive(false);
			OnScreenHidden?.Invoke(this);
		}

		protected virtual async UniTask ExecuteShowTransition(CancellationToken cancellationToken)
		{
			base._currentAnimation?.Kill();

			switch (_showTransition)
			{
				case ScreenTransitionType.Fade:
					await _canvasGroup.FadeToAsync(1f, _showDuration, _showEase, cancellationToken);
					break;

				case ScreenTransitionType.SlideFromBottom:
					await _rectTransform.SlideInFromBottom(_canvasGroup, _showDuration, _transitionDistance, cancellationToken);
					break;

				case ScreenTransitionType.SlideFromTop:
					await SlideFromDirection(Vector2.up, cancellationToken);
					break;

				case ScreenTransitionType.SlideFromLeft:
					await SlideFromDirection(Vector2.left, cancellationToken);
					break;

				case ScreenTransitionType.SlideFromRight:
					await SlideFromDirection(Vector2.right, cancellationToken);
					break;

				case ScreenTransitionType.Scale:
					await _rectTransform.PopupScale(_canvasGroup, _showDuration, cancellationToken);
					break;

				case ScreenTransitionType.Custom:
					await CustomShowTransition(cancellationToken);
					break;
			}
		}

		protected virtual async UniTask ExecuteHideTransition(CancellationToken cancellationToken)
		{
			base._currentAnimation?.Kill();

			switch (_hideTransition)
			{
				case ScreenTransitionType.Fade:
					await _canvasGroup.FadeToAsync(0f, _hideDuration, _hideEase, cancellationToken);
					break;

				case ScreenTransitionType.SlideFromBottom:
					await _rectTransform.SlideOutToBottom(_canvasGroup, _hideDuration, _transitionDistance, cancellationToken);
					break;

				case ScreenTransitionType.SlideFromTop:
					await SlideToDirection(Vector2.up, cancellationToken);
					break;

				case ScreenTransitionType.SlideFromLeft:
					await SlideToDirection(Vector2.left, cancellationToken);
					break;

				case ScreenTransitionType.SlideFromRight:
					await SlideToDirection(Vector2.right, cancellationToken);
					break;

				case ScreenTransitionType.Scale:
					await _rectTransform.PopupScaleOut(_canvasGroup, _hideDuration, cancellationToken);
					break;

				case ScreenTransitionType.Custom:
					await CustomHideTransition(cancellationToken);
					break;
			}
		}

		protected virtual async UniTask SlideFromDirection(Vector2 direction, CancellationToken cancellationToken)
		{
			Vector2 targetPosition = _rectTransform.anchoredPosition;
			Vector2 startPosition = targetPosition - direction * _transitionDistance;

			_rectTransform.anchoredPosition = startPosition;
			_canvasGroup.alpha = 0f;

			Sequence sequence = DOTween.Sequence();
			sequence.Append(_rectTransform.DOAnchorPos(targetPosition, _showDuration).SetEase(_showEase));
			sequence.Join(_canvasGroup.DOFade(1f, _showDuration).SetEase(_showEase));

			await sequence.ToUniTask(cancellationToken: cancellationToken);
		}

		protected virtual async UniTask SlideToDirection(Vector2 direction, CancellationToken cancellationToken)
		{
			Vector2 startPosition = _rectTransform.anchoredPosition;
			Vector2 targetPosition = startPosition + direction * _transitionDistance;

			Sequence sequence = DOTween.Sequence();
			sequence.Append(_rectTransform.DOAnchorPos(targetPosition, _hideDuration).SetEase(_hideEase));
			sequence.Join(_canvasGroup.DOFade(0f, _hideDuration).SetEase(_hideEase));

			await sequence.ToUniTask(cancellationToken: cancellationToken);
		}

		protected virtual UniTask CustomShowTransition(CancellationToken cancellationToken)
		{
			// Override in derived classes to implement custom show transitions
			return _canvasGroup.FadeToAsync(1f, _showDuration, _showEase, cancellationToken);
		}

		protected virtual UniTask CustomHideTransition(CancellationToken cancellationToken)
		{
			// Override in derived classes to implement custom hide transitions
			return _canvasGroup.FadeToAsync(0f, _hideDuration, _hideEase, cancellationToken);
		}
	}
}
