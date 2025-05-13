using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.UI.Common;
using UnityEngine;

namespace ROC.UI.MainMenu.LevelSelection
{
	public class LevelSelectionView : BaseView, ILevelSelectionView
	{
		[SerializeField] private Transform _levelsContainer;
		[SerializeField] private UIButton _backButton;

		private readonly List<LevelButton> _levelButtons = new List<LevelButton>();
		private Action _onBackButtonClicked;
		private Action<int> _onLevelSelected;

		public GameObject GameObject => gameObject;

		protected override void Awake()
		{
			base.Awake();

			// Find and collect all level buttons that are already in the prefab
			CollectLevelButtons();

			// Setup listeners
			_backButton.Button.onClick.AddListener(HandleBackButtonClicked);
			foreach (var button in _levelButtons)
			{
				button.OnLevelSelected += HandleLevelSelected;
			}
		}

		protected override void OnDestroy()
		{
			_backButton.Button.onClick.RemoveListener(HandleBackButtonClicked);

			foreach (var button in _levelButtons)
			{
				button.OnLevelSelected -= HandleLevelSelected;
			}

			_levelButtons.Clear();
			base.OnDestroy();
		}

		private void CollectLevelButtons()
		{
			// Find all level buttons that are already in the prefab
			LevelButton[] existingButtons = _levelsContainer.GetComponentsInChildren<LevelButton>(true);
			_levelButtons.AddRange(existingButtons);
		}

		#region ILevelSelectionView Implementation

		public void SetBackButtonClickListener(Action callback)
		{
			_onBackButtonClicked = callback;
		}

		public void SetLevelButtonsState(bool[] unlockedLevels)
		{
			for (int i = 0; i < _levelButtons.Count && i < unlockedLevels.Length; i++)
			{
				_levelButtons[i].Initialize(i, unlockedLevels[i]);
			}
		}

		public void SetLevelSelectedListener(Action<int> callback)
		{
			_onLevelSelected = callback;
		}

		#endregion

		#region Event Handlers

		private void HandleBackButtonClicked()
		{
			_onBackButtonClicked?.Invoke();
		}

		private void HandleLevelSelected(int levelIndex)
		{
			_onLevelSelected?.Invoke(levelIndex);
		}

		#endregion
	}
}