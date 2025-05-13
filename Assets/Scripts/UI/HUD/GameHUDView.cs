using System;
using System.Collections.Generic;
using ROC.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.HUD
{
	public class GameHUDView : BaseView, IGameHUDView
	{
		[Header("Controls")]
		[SerializeField] private Button _leftButton;
		[SerializeField] private Button _rightButton;
		[SerializeField] private Button _jumpButton;

		[Header("Lives")]
		[SerializeField] private GameObject _lifeImagePrefab;
		[SerializeField] private Transform _livesContainer;

		[Header("Stats")]
		[SerializeField] private TextMeshProUGUI _scoreText;
		[SerializeField] private TextMeshProUGUI _heightText;
		[SerializeField] private TextMeshProUGUI _speedText;

		private readonly List<GameObject> _lifeImages = new List<GameObject>();

		private Action _onLeftButtonPressed;
		private Action _onLeftButtonReleased;
		private Action _onRightButtonPressed;
		private Action _onRightButtonReleased;
		private Action _onJumpButtonPressed;
		private Action _onJumpButtonReleased;

		public GameObject GameObject => gameObject;

		protected override void InitializeView()
		{
			// Set up button event handlers
			_leftButton.AddEventHandlers(OnLeftButtonDown, OnLeftButtonUp);
			_rightButton.AddEventHandlers(OnRightButtonDown, OnRightButtonUp);
			_jumpButton.AddEventHandlers(OnJumpButtonDown, OnJumpButtonUp);
		}

		protected override void OnDestroy()
		{
			// Remove button event handlers
			_leftButton.RemoveEventHandlers(OnLeftButtonDown, OnLeftButtonUp);
			_rightButton.RemoveEventHandlers(OnRightButtonDown, OnRightButtonUp);
			_jumpButton.RemoveEventHandlers(OnJumpButtonDown, OnJumpButtonUp);

			// Clean up life images
			foreach (var lifeImage in _lifeImages)
			{
				Destroy(lifeImage);
			}
			_lifeImages.Clear();

			base.OnDestroy();
		}

		public void SetLeftButtonListeners(Action onPress, Action onRelease)
		{
			_onLeftButtonPressed = onPress;
			_onLeftButtonReleased = onRelease;
		}

		public void SetRightButtonListeners(Action onPress, Action onRelease)
		{
			_onRightButtonPressed = onPress;
			_onRightButtonReleased = onRelease;
		}

		public void SetJumpButtonListeners(Action onPress, Action onRelease)
		{
			_onJumpButtonPressed = onPress;
			_onJumpButtonReleased = onRelease;
		}

		public void UpdateLives(int current, int max)
		{
			// Ensure we have the correct number of life images
			while (_lifeImages.Count < max)
			{
				GameObject lifeImage = Instantiate(_lifeImagePrefab, _livesContainer);
				_lifeImages.Add(lifeImage);
			}

			// Remove extra life images if needed
			while (_lifeImages.Count > max)
			{
				int lastIndex = _lifeImages.Count - 1;
				Destroy(_lifeImages[lastIndex]);
				_lifeImages.RemoveAt(lastIndex);
			}

			// Update visibility of life images
			for (int i = 0; i < _lifeImages.Count; i++)
			{
				_lifeImages[i].SetActive(i < current);
			}
		}

		public void UpdateScore(int score)
		{
			_scoreText.text = $"Score: {score}";
		}

		public void UpdateHeight(float height)
		{
			_heightText.text = $"Height: {height:0.0}m";
		}

		public void UpdateSpeed(float speed)
		{
			_speedText.text = $"Speed: {speed:0.0}m/s";
		}

		private void OnLeftButtonDown()
		{
			_onLeftButtonPressed?.Invoke();
		}

		private void OnLeftButtonUp()
		{
			_onLeftButtonReleased?.Invoke();
		}

		private void OnRightButtonDown()
		{
			_onRightButtonPressed?.Invoke();
		}

		private void OnRightButtonUp()
		{
			_onRightButtonReleased?.Invoke();
		}

		private void OnJumpButtonDown()
		{
			_onJumpButtonPressed?.Invoke();
		}

		private void OnJumpButtonUp()
		{
			_onJumpButtonReleased?.Invoke();
		}
	}

	// Helper class for button event handling
	public static class ButtonExtensions
	{
		public static void AddEventHandlers(this Button button, Action onDown, Action onUp)
		{
			var pointerHandler = button.gameObject.GetOrAddComponent<ButtonPointerHandler>();
			pointerHandler.OnPointerDown += onDown;
			pointerHandler.OnPointerUp += onUp;
		}

		public static void RemoveEventHandlers(this Button button, Action onDown, Action onUp)
		{
			var pointerHandler = button.gameObject.GetComponent<ButtonPointerHandler>();
			if (pointerHandler != null)
			{
				pointerHandler.OnPointerDown -= onDown;
				pointerHandler.OnPointerUp -= onUp;
			}
		}

		private static T GetOrAddComponent<T>(this GameObject go) where T : Component
		{
			T component = go.GetComponent<T>();
			if (component == null)
			{
				component = go.AddComponent<T>();
			}
			return component;
		}
	}
}