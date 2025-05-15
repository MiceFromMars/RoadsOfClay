using ROC.Core.Assets;
using ROC.Core.Events;
using ROC.Core.StateMachine;
using ROC.Core.StateMachine.States;
using ROC.Data.SaveLoad;
using ROC.Game.PlayerInput;
using ROC.Game.Levels;
using ROC.Game.PlayerBeh;
using ROC.Game.Enemy;
using ROC.Game.Cam;
using ROC.UI;
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

			// Register GameStateMachine first using the new constructor
			builder.Register<GameStateMachine>(Lifetime.Singleton);

			builder.Register<AssetsProvider>(Lifetime.Singleton)
				.As<IAssetsProvider>()
				.AsSelf();

			builder.Register<SaveLoadService>(Lifetime.Singleton)
				.As<ISaveLoadService>()
				.AsSelf();

			builder.Register<InputProvider>(Lifetime.Singleton)
				.As<IInputProvider>()
				.As<ITickable>();
		}

		private void RegisterUISystem(IContainerBuilder builder)
		{
			builder.Register<UIProvider>(Lifetime.Singleton)
				.WithParameter("uiRoot", _uiRoot)
				.As<IUIProvider>()
				.As<IInitializable>()
				.AsSelf();
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
			// Register all the states
			builder.Register<BootstrapState>(Lifetime.Singleton)
				.As<IState>()
				.AsSelf();

			builder.Register<MainMenuState>(Lifetime.Singleton)
				.As<IState>()
				.AsSelf();

			builder.Register<GameplayState>(Lifetime.Singleton)
				.As<IState>()
				.AsSelf();
		}
	}
}
