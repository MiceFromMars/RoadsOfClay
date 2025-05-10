using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ROC.UI.Common
{
	public class UIInputButton : UIButton, IPointerDownHandler, IPointerUpHandler
	{
		public event Action OnPress;
		public event Action OnRelease;

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!Button.interactable)
				return;

			OnPress?.Invoke();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (!Button.interactable)
				return;

			OnRelease?.Invoke();
		}
	}
}