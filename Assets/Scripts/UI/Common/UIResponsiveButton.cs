using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ROC.Core.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ROC.UI.Common
{
	[RequireComponent(typeof(Button))]
	public class UIResponsiveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[Header("Visual Feedback")]
		[SerializeField] private bool _useScaleAnimation = true;
		[SerializeField] private float _pressedScale = 0.95f;
		[SerializeField] private float _hoverScale = 1.05f;
		[SerializeField] private float _scaleAnimationDuration = 0.1f;

		[Header("Color Transition")]
		[SerializeField] private bool _useColorTransition = true;
		[SerializeField] private Color _normalColor = Color.white;
		[SerializeField] private Color _pressedColor = new Color(0.8f, 0.8f, 0.8f);
		[SerializeField] private Color _hoverColor = new Color(0.9f, 0.9f, 0.9f);
		[SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f);
		[SerializeField] private float _colorTransitionDuration = 0.1f;

		[Header("Sound")]
		[SerializeField] private bool _playSound = true;
		[SerializeField] private string _clickSoundEventId = "button_click";
		[SerializeField] private string _hoverSoundEventId = "button_hover";

		[Header("References")]
		[SerializeField] private Image _targetGraphic;

		private Button _button;
		private RectTransform _rectTransform;
		private Vector3 _originalScale;
		private CancellationTokenSource _animationCts;
		private Sequence _currentAnimation;
		private bool _isInteractable = true;
		private IEventBus _eventBus;

		public bool IsInteractable
		{
			get => _isInteractable;
			set
			{
				_isInteractable = value;
				_button.interactable = value;
				UpdateVisualState(value ? ButtonState.Normal : ButtonState.Disabled);
			}
		}

		private enum ButtonState
		{
			Normal,
			Pressed,
			Hover,
			Disabled
		}

		private void Awake()
		{
			_button = GetComponent<Button>();
			_rectTransform = transform as RectTransform;

			if (_targetGraphic == null)
			{
				_targetGraphic = GetComponent<Image>();
			}

			_originalScale = _rectTransform.localScale;
			_animationCts = new CancellationTokenSource();
		}

		private void OnEnable()
		{
			// Try to find EventBus in the scene
			if (_eventBus == null)
			{
				_eventBus = FindObjectOfType<MonoBehaviour>() as IEventBus;
			}

			UpdateVisualState(_isInteractable ? ButtonState.Normal : ButtonState.Disabled);
		}

		private void OnDisable()
		{
			CancelCurrentAnimation();
		}

		private void OnDestroy()
		{
			_animationCts?.Cancel();
			_animationCts?.Dispose();
			_animationCts = null;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!_isInteractable) return;
			UpdateVisualState(ButtonState.Pressed);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (!_isInteractable) return;
			UpdateVisualState(ButtonState.Normal);

			if (_playSound && eventData.pointerPress == gameObject)
			{
				PlaySound(_clickSoundEventId);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!_isInteractable) return;
			UpdateVisualState(ButtonState.Hover);

			if (_playSound)
			{
				PlaySound(_hoverSoundEventId);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!_isInteractable) return;
			UpdateVisualState(ButtonState.Normal);
		}

		private void PlaySound(string soundId)
		{
			if (_eventBus != null)
			{
				_eventBus.Fire(new PlaySoundEvent { SoundId = soundId });
			}
		}

		private void UpdateVisualState(ButtonState state)
		{
			CancelCurrentAnimation();

			// Create a new token
			var token = _animationCts.Token;

			// Handle scale animation
			if (_useScaleAnimation)
			{
				Vector3 targetScale = _originalScale;

				switch (state)
				{
					case ButtonState.Pressed:
						targetScale = _originalScale * _pressedScale;
						break;
					case ButtonState.Hover:
						targetScale = _originalScale * _hoverScale;
						break;
				}

				_currentAnimation = DOTween.Sequence();
				_currentAnimation.Append(_rectTransform.DOScale(targetScale, _scaleAnimationDuration).SetEase(Ease.OutQuad));
			}

			// Handle color transition
			if (_useColorTransition && _targetGraphic != null)
			{
				Color targetColor = _normalColor;

				switch (state)
				{
					case ButtonState.Pressed:
						targetColor = _pressedColor;
						break;
					case ButtonState.Hover:
						targetColor = _hoverColor;
						break;
					case ButtonState.Disabled:
						targetColor = _disabledColor;
						break;
				}

				if (_currentAnimation == null)
				{
					_currentAnimation = DOTween.Sequence();
				}

				_currentAnimation.Join(_targetGraphic.DOColor(targetColor, _colorTransitionDuration).SetEase(Ease.OutQuad));
			}

			// Play the animation
			if (_currentAnimation != null)
			{
				_currentAnimation.SetUpdate(true);
				_currentAnimation.Play();
			}
		}

		private void CancelCurrentAnimation()
		{
			if (_currentAnimation != null && _currentAnimation.IsActive())
			{
				_currentAnimation.Kill();
				_currentAnimation = null;
			}
		}
	}

	// A simple sound event for demonstration purposes
	public struct PlaySoundEvent : IEvent
	{
		public string SoundId;
	}
}