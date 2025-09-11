using Game.Entities;
using Game.Field;
using Game.User;
using Multiplayer.Client;
using Multiplayer.UI;
using Zenject;

namespace Multiplayer
{
    public class MultiplayerGameMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            FieldInstaller.Install(Container);
            EntitiesInstaller.Install(Container);
            
            Container.Bind(typeof(Game.States.IGameSubstatesInstaller), typeof(Game.States.IGameSubstateResolver))
                .FromSubContainerResolve()
                .ByNewGameObjectMethod(InstallSubcontainer)
                .AsSingle();
            
            Container.BindFactory<UserRoundModel,UserRoundModel.Factory>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<UserRoundModel.Provider>()
                .AsSingle();
            
            Container.BindInterfacesTo<UIMultiplayerController>()
                .AsSingle();
            
            Container.BindInterfacesTo<ClientSubstateController>()
                .AsSingle()
                .NonLazy();
            
#if LAN_STATE_MACHINE_DEBUG
            BindDebugContracts(Container);
#endif
        }
        private void InstallSubcontainer(DiContainer container)
        {
            new ClientStateMachineInstaller().Install(container);
        }

        private void BindDebugContracts(DiContainer container)
        {
            container.BindInterfacesTo<ClientStateProviderDebug>()
                .AsSingle();
            container.BindInterfacesTo<ClientStateMachineDebug>()
                .AsSingle()
                .NonLazy();
        }
    }
}