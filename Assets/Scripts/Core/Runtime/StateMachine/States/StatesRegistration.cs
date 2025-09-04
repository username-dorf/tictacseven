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
            Container.InstallState<MenuState>();
            Container.InstallState<GameState>();
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
                .NonLazy();
        }
    }
}