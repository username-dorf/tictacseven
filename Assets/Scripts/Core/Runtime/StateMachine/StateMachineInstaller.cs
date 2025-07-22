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
            
            StatesRegistration.Install(Container);
        }
    }
}
