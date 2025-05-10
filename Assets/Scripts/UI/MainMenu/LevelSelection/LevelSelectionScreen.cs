using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.Data.SaveLoad;
using ROC.UI.Common;
using UnityEngine;
using VContainer;

namespace ROC.UI.MainMenu.LevelSelection
{
	public class LevelSelectionScreen : UIScreen
	{
		[SerializeField] private Transform _levelsContainer;
		[SerializeField] private UIButton _backButton;

		private readonly IEventBus _eventBus;
		private readonly ISaveLoadService _saveLoadService;
		private readonly List<LevelButton> _levelButtons = new List<LevelButton>();
		private bool _initialized;
		private PlayerProgressData _playerProgress;

		[Inject]
		public LevelSelectionScreen(IEventBus eventBus, ISaveLoadService saveLoadService)
		{
			_eventBus = eventBus;
			_saveLoadService = saveLoadService;
		}

		protected override void Awake()
		{
			base.Awake();
			_backButton.Button.onClick.AddListener(OnBackButtonClicked);

			// Find and collect all level buttons that are already in the prefab
			CollectLevelButtons();
		}

		protected override void OnDestroy()
		{
			_backButton.Button.onClick.RemoveListener(OnBackButtonClicked);

			foreach (var button in _levelButtons)
			{
				button.OnLevelSelected -= OnLevelSelected;
			}

			_levelButtons.Clear();

			base.OnDestroy();
			Dispose();
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			// Load player progress before showing the screen
			await LoadPlayerProgress(cancellationToken);

			await base.Show(cancellationToken);

			if (!_initialized)
			{
				InitializeLevelButtons();
				_initialized = true;
			}

			_eventBus.Fire(new LevelSelectionOpenedEvent());
		}

		private async UniTask LoadPlayerProgress(CancellationToken cancellationToken)
		{
			_playerProgress = await _saveLoadService.LoadProgress(cancellationToken);
		}

		private void CollectLevelButtons()
		{
			// Find all level buttons that are already in the prefab
			LevelButton[] existingButtons = _levelsContainer.GetComponentsInChildren<LevelButton>(true);
			_levelButtons.AddRange(existingButtons);
		}

		private void InitializeLevelButtons()
		{
			for (int i = 0; i < _levelButtons.Count; i++)
			{
				int levelIndex = i;
				bool isUnlocked = IsLevelUnlocked(levelIndex);

				_levelButtons[i].Initialize(levelIndex, isUnlocked);
				_levelButtons[i].OnLevelSelected += OnLevelSelected;
			}
		}

		private bool IsLevelUnlocked(int levelIndex)
		{
			// First level is always unlocked
			if (levelIndex == 0)
				return true;

			// Check if this level is explicitly marked as unlocked in player progress
			var levelProgress = _playerProgress.LevelProgress.FirstOrDefault(lp => lp.LevelIndex == levelIndex);
			if (levelProgress != null && levelProgress.IsUnlocked)
				return true;

			// Check if previous level is completed (which should unlock this one)
			// This ensures levels unlock sequentially
			var previousLevelProgress = _playerProgress.LevelProgress.FirstOrDefault(lp => lp.LevelIndex == levelIndex - 1);
			return previousLevelProgress != null && previousLevelProgress.IsUnlocked;
		}

		private void OnLevelSelected(int levelIndex)
		{
			_eventBus.Fire(new LevelSelectedEvent { LevelIndex = levelIndex });
		}

		private void OnBackButtonClicked()
		{
			_eventBus.Fire(new MainMenuOpenedEvent());
		}
	}
}