using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ROC.Game.Player
{
	public interface IPlayerInput
	{
		float HorizontalInput { get; }
		bool JumpInput { get; }
		void Initialize(PlayerBehavior playerBehavior);
		void Cleanup();
		void Dispose();
	}

	public class PlayerInputHandler : MonoBehaviour, IPlayerInput, IDisposable
	{
		[SerializeField] private Button _leftButton;
		[SerializeField] private Button _rightButton;
		[SerializeField] private Button _jumpButton;

		private PlayerBehavior _playerBehavior;
		private bool _isLeftPressed;
		private bool _isRightPressed;
		private bool _isJumpPressed;

		public float HorizontalInput
		{
			get
			{
				float input = 0f;
				if (_isLeftPressed) input -= 1f;
				if (_isRightPressed) input += 1f;
				return input;
			}
		}

		public bool JumpInput => _isJumpPressed;

		public void Initialize(PlayerBehavior playerBehavior)
		{
			_playerBehavior = playerBehavior;
			SetupButtons();
		}

		public void Cleanup()
		{
			Dispose();
			_playerBehavior = null;
		}

		private void SetupButtons()
		{
			if (_leftButton != null)
			{
				AddPointerHandlers(_leftButton.gameObject,
					() => _isLeftPressed = true,
					() => _isLeftPressed = false);
			}

			if (_rightButton != null)
			{
				AddPointerHandlers(_rightButton.gameObject,
					() => _isRightPressed = true,
					() => _isRightPressed = false);
			}

			if (_jumpButton != null)
			{
				AddPointerHandlers(_jumpButton.gameObject,
					() => _isJumpPressed = true,
					() => _isJumpPressed = false);
			}
		}

		private void AddPointerHandlers(GameObject buttonObj, Action onDown, Action onUp)
		{
			EventTrigger trigger = buttonObj.GetComponent<EventTrigger>() ?? buttonObj.AddComponent<EventTrigger>();
			trigger.triggers.Clear(); // Clear existing triggers to avoid duplicates

			EventTrigger.Entry pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
			pointerDown.callback.AddListener((_) => onDown());
			trigger.triggers.Add(pointerDown);

			EventTrigger.Entry pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
			pointerUp.callback.AddListener((_) => onUp());
			trigger.triggers.Add(pointerUp);
		}

		private void Update()
		{
			if (_playerBehavior == null)
				return;

			_playerBehavior.SetInput(HorizontalInput, JumpInput);
		}

		public void Dispose()
		{
			CleanupEventTriggers();
		}

		private void CleanupEventTriggers()
		{
			CleanupEventTrigger(_leftButton);
			CleanupEventTrigger(_rightButton);
			CleanupEventTrigger(_jumpButton);
		}

		private void CleanupEventTrigger(Button button)
		{
			if (button != null)
			{
				EventTrigger trigger = button.GetComponent<EventTrigger>();
				if (trigger != null)
				{
					trigger.triggers.Clear();
				}
			}
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}