using System;
using Zenject;

namespace Core.StateMachine
{
    public class StatesRegistration : Installer<StatesRegistration>
    {
        public override void InstallBindings()
        {
            BindStates();
        }
        private void BindStates()
        {
            Container.InstallState<BootstrapState>();
            Container.InstallState<PersistantResourcesLoadState>();
            Container.InstallState<MenuState>();
            Container.InstallState<GameState>();
            Container.InstallState<MultiplayerGameState>();
        }
    }

    public static class StatesRegistrationExtensions
    {
        public static void InstallState<TState>(this DiContainer diContainer) where TState : IState
        {
            diContainer.Bind<IState>()
                .WithId(typeof(TState).Name)
                .To<TState>()
                .AsSingle()
                .OnInstantiated<TState>((ctx, state) =>
                {
                    var disp = ctx.Container.Resolve<DisposableManager>();
                    if (state is IDisposable d)
                        disp.Add(d);
                });

        }
    }
}