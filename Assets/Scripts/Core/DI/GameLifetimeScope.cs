using Cysharp.Threading.Tasks;
using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.Core.StateMachine;
using ROC.Core.StateMachine.States;
using ROC.Data.Config;
using ROC.Data.SaveLoad;
using ROC.Game.Levels;
using ROC.Game.Player;
using ROC.Game.Enemy;
using ROC.UI;
using ROC.UI.HUD;
using ROC.UI.MainMenu;
using ROC.UI.MainMenu.LevelSelection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ROC.Core.DI
{
	public class GameLifetimeScope : LifetimeScope
	{
		[SerializeField] private Transform _uiRoot;

		protected override void Configure(IContainerBuilder builder)
		{
			// Core
			builder.Register<LoggingService>(Lifetime.Singleton).As<ILoggingService>();
			builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
			builder.Register<GameStateMachine>(Lifetime.Singleton);

			// Asset Provider registration with proper disposal handled automatically by VContainer
			builder.Register<AssetsProvider>(Lifetime.Singleton)
				.As<IAssetsProvider>()
				.AsSelf();

			// SaveLoadService registration with proper disposal handled automatically by VContainer
			builder.Register<SaveLoadService>(Lifetime.Singleton)
				.As<ISaveLoadService>()
				.AsSelf();

			// UI Service and screens
			RegisterUISystem(builder);

			// Game
			RegisterGameSystems(builder);

			// States
			RegisterGameStates(builder);

			// Entrypoint
			builder.RegisterEntryPoint<GameEntryPoint>();
		}

		private void RegisterUISystem(IContainerBuilder builder)
		{
			// Register UI Service
			builder.Register<UIService>(Lifetime.Singleton)
				.WithParameter("uiRoot", _uiRoot)
				.As<IUIService>()
				.AsSelf();

			// Register UI Screens - only register types that need specific dependencies
			// Most screens will be loaded via Addressables
			builder.Register<MainMenuScreen>(Lifetime.Transient);
			builder.Register<LevelSelectionScreen>(Lifetime.Transient);
			builder.Register<GameHUD>(Lifetime.Transient);
			builder.Register<GameOverScreen>(Lifetime.Transient);
		}

		private void RegisterGameSystems(IContainerBuilder builder)
		{
			builder.Register<LevelLoader>(Lifetime.Singleton).As<ILevelLoader>();
			builder.Register<LevelProvider>(Lifetime.Singleton).As<ILevelProvider>();
			builder.Register<EnemyFactory>(Lifetime.Singleton).As<IEnemyFactory>();
			builder.Register<PlayerProvider>(Lifetime.Singleton).As<IPlayerProvider>();
			builder.Register<CameraProvider>(Lifetime.Singleton).As<ICameraProvider>();
		}

		private void RegisterGameStates(IContainerBuilder builder)
		{
			builder.Register<BootstrapState>(Lifetime.Singleton);
			builder.Register<MainMenuState>(Lifetime.Singleton);
			builder.Register<GameplayState>(Lifetime.Singleton);
		}
	}
}
