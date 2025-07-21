using Zenject;

namespace Core.StateMachine
{
    public class StateMachineInstaller : Installer<StateMachineInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<StateMachine>()
                .AsSingle();
            Container.Bind<StateFactory>()
                .AsSingle();
            
            BindStates();
        }
        
        public void BindStates()
        {
            Container.Bind<IState>()
                .WithId(nameof(BootstrapState))
                .To<BootstrapState>()
                .AsSingle()
                .NonLazy();
        }
    }
}
