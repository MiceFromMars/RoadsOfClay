using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.Core.StateMachine;
using ROC.Core.StateMachine.States;
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
			RegisterCoreSystems(builder);

			RegisterUISystem(builder);

			RegisterGameSystems(builder);

			RegisterGameStates(builder);

			builder.RegisterEntryPoint<GameStateMachineInitializer>();
		}

		private void RegisterCoreSystems(IContainerBuilder builder)
		{
			builder.Register<LoggingService>(Lifetime.Singleton).As<ILoggingService>();
			builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
			builder.Register<GameStateMachine>(Lifetime.Singleton);

			builder.Register<AssetsProvider>(Lifetime.Singleton)
				.As<IAssetsProvider>()
				.AsSelf();

			builder.Register<SaveLoadService>(Lifetime.Singleton)
				.As<ISaveLoadService>()
				.AsSelf();
		}

		private void RegisterUISystem(IContainerBuilder builder)
		{
			builder.Register<UIService>(Lifetime.Singleton)
				.WithParameter("uiRoot", _uiRoot)
				.As<IUIService>()
				.AsSelf();

			builder.Register<MainMenuScreen>(Lifetime.Singleton);
			builder.Register<LevelSelectionScreen>(Lifetime.Singleton);
			builder.Register<GameHUD>(Lifetime.Singleton);
			builder.Register<GameOverScreen>(Lifetime.Singleton);
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
