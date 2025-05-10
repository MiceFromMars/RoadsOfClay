using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ROC.UI.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ROC.UI.Common
{
	[RequireComponent(typeof(Button))]
	public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private float _clickScaleDuration = 0.1f;
		[SerializeField] private float _clickScaleAmount = 0.95f;
		[SerializeField] private float _hoverScaleDuration = 0.2f;
		[SerializeField] private float _hoverScaleAmount = 1.05f;

		private Button _button;
		private RectTransform _rectTransform;
		private Sequence _currentAnimation;
		private CancellationTokenSource _cts = new CancellationTokenSource();

		public Button Button => _button;

		protected virtual void Awake()
		{
			_button = GetComponent<Button>();
			_rectTransform = GetComponent<RectTransform>();

			_button.onClick.AddListener(OnClick);
		}

		protected virtual void OnDestroy()
		{
			_button.onClick.RemoveListener(OnClick);
			_currentAnimation?.Kill();
			_cts?.Cancel();
			_cts?.Dispose();
		}

		protected virtual void OnClick()
		{
			PlayClickAnimation().Forget();
		}

		private async UniTask PlayClickAnimation()
		{
			_currentAnimation?.Kill();
			_cts.Cancel();
			_cts = new CancellationTokenSource();

			try
			{
				await _rectTransform.ScaleToAsync(Vector3.one * _clickScaleAmount, _clickScaleDuration, Ease.OutQuad, _cts.Token);
				await _rectTransform.ScaleToAsync(Vector3.one, _clickScaleDuration, Ease.InQuad, _cts.Token);
			}
			catch (OperationCanceledException)
			{
				// Animation was canceled, which is expected
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!_button.interactable)
				return;

			_currentAnimation?.Kill();
			_cts.Cancel();
			_cts = new CancellationTokenSource();

			_rectTransform.ScaleToAsync(Vector3.one * _hoverScaleAmount, _hoverScaleDuration, Ease.OutQuad, _cts.Token).Forget();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!_button.interactable)
				return;

			_currentAnimation?.Kill();
			_cts.Cancel();
			_cts = new CancellationTokenSource();

			_rectTransform.ScaleToAsync(Vector3.one, _hoverScaleDuration, Ease.InQuad, _cts.Token).Forget();
		}
	}
}