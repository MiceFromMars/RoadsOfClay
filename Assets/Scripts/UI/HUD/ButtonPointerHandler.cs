using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ROC.UI.HUD
{
	public class ButtonPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public event Action OnPointerDown;
		public event Action OnPointerUp;

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			OnPointerDown?.Invoke();
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			OnPointerUp?.Invoke();
		}
	}
}