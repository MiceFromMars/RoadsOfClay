using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ROC.Core.Events;
using ROC.Data.SaveLoad;
using ROC.UI.Common;

namespace ROC.UI.MainMenu.LevelSelection
{
	public class LevelSelectionPresenter : BasePresenter<ILevelSelectionView>
	{
		private readonly ISaveLoadService _saveLoadService;
		private PlayerProgressData _playerProgress;

		public LevelSelectionPresenter(
			ILevelSelectionView view,
			IEventBus eventBus,
			ISaveLoadService saveLoadService) : base(view, eventBus)
		{
			_saveLoadService = saveLoadService;
		}

		public override void Initialize()
		{
			base.Initialize();

			// Set up listeners for view actions
			_view.SetBackButtonClickListener(OnBackButtonClicked);
			_view.SetLevelSelectedListener(OnLevelSelected);
		}

		protected override void SubscribeToEvents()
		{
			base.SubscribeToEvents();
		}

		protected override void UnsubscribeFromEvents()
		{
			base.UnsubscribeFromEvents();
		}

		public override async UniTask Show(CancellationToken cancellationToken = default)
		{
			// Load player progress
			await LoadPlayerProgress(cancellationToken);

			// Set the state of level buttons based on progress
			UpdateLevelButtonsState();

			await base.Show(cancellationToken);
			_eventBus.Fire(new LevelSelectionOpenedEvent());
		}

		private async UniTask LoadPlayerProgress(CancellationToken cancellationToken)
		{
			_playerProgress = await _saveLoadService.LoadProgress(cancellationToken);
		}

		private void UpdateLevelButtonsState()
		{
			// Find out how many level buttons we have in the view
			int levelsCount = _playerProgress.LevelProgress.Count;
			bool[] unlockedLevels = new bool[levelsCount];

			// Set unlock state for each level
			for (int i = 0; i < levelsCount; i++)
			{
				unlockedLevels[i] = IsLevelUnlocked(i);
			}

			_view.SetLevelButtonsState(unlockedLevels);
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