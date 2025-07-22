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
            InstallState<BootstrapState>(Container);
            InstallState<MenuState>(Container);
            InstallState<GameState>(Container);
        }
        private void InstallState<TState>(DiContainer diContainer) where TState : IState
        {
            diContainer.Bind<IState>()
                .WithId(typeof(TState).Name)
                .To<TState>()
                .AsSingle()
                .NonLazy();
        }
    }
}