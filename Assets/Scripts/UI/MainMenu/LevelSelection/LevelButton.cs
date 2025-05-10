using System;
using ROC.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROC.UI.MainMenu.LevelSelection
{
	public class LevelButton : UIButton
	{
		[SerializeField] private TextMeshProUGUI _levelNumberText;
		[SerializeField] private GameObject _lockIcon;
		[SerializeField] private Image _buttonBackground;
		[SerializeField] private Color _lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
		[SerializeField] private Color _unlockedColor = Color.white;

		private int _levelIndex;
		private bool _isUnlocked;

		public event Action<int> OnLevelSelected;

		public bool IsUnlocked => _isUnlocked;

		public void Initialize(int levelIndex, bool isUnlocked = false)
		{
			_levelIndex = levelIndex;
			_levelNumberText.text = (levelIndex + 1).ToString();
			SetUnlocked(isUnlocked);
		}

		public void SetUnlocked(bool isUnlocked)
		{
			_isUnlocked = isUnlocked;

			// Update visuals
			if (_lockIcon != null)
				_lockIcon.SetActive(!isUnlocked);

			if (_buttonBackground != null)
				_buttonBackground.color = isUnlocked ? _unlockedColor : _lockedColor;

			// Update interactability
			Button.interactable = isUnlocked;
		}

		protected override void OnClick()
		{
			// Only respond to clicks if unlocked
			if (!_isUnlocked)
				return;

			base.OnClick();
			OnLevelSelected?.Invoke(_levelIndex);
		}
	}
}