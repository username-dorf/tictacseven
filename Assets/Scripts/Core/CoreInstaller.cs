using Core.Common;
using Core.StateMachine;
using Core.UI;
using Core.User;
using Zenject;

namespace Core
{
    public class CoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            StateMachineInstaller.Install(Container);
            UIInstaller.Install(Container);
            UserInstaller.Install(Container);
            CommonInstaller.Install(Container);
            BindBootstrap(Container);
        }

        private void BindBootstrap(DiContainer diContainer)
        {
            diContainer.BindInterfacesTo<Bootstrap>()
                .AsSingle()
                .NonLazy();
        }
    }
}
