using Core.Audio;
using Core.Common;
using Core.StateMachine;
using Core.UI;
using Core.User;
using Core.VFX;
using UnityEngine.Scripting;
using Zenject;

namespace Core
{
    [Preserve] 
    public class CoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            StateMachineInstaller.Install(Container);
            UIInstaller.Install(Container);
            UserInstaller.Install(Container);
            CommonInstaller.Install(Container);
            AudioInstaller.Install(Container);
            FxInstaller.Install(Container);
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
