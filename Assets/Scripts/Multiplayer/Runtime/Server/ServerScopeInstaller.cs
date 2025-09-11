using Game.Field;
using UniState;
using Zenject;
using Multiplayer.Server.States;

namespace Multiplayer.Server
{
    public class ServerScopeInstaller: MonoInstaller<ServerScopeInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ServerFacade>()
                .FromComponentOnRoot()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ServerService>()
                .AsSingle()
                .NonLazy();

            Container.BindFactory<ServerSubstateScope, ServerSubstateScope.Factory>()
                .FromSubContainerResolve()
                .ByMethod(InstallMatchSubcontainer)
                .AsSingle();
#if LAN_STATE_MACHINE_DEBUG
            BindDebugContracts(Container);
#endif
        }

        private void InstallMatchSubcontainer(DiContainer sub)
        {
            sub.BindInterfacesAndSelfTo<ServerSubstateScope>().AsSingle();
            sub.BindInterfacesTo<ServerActiveClientProvider>().AsSingle();
            sub.BindInterfacesTo<ServerClientsProvider>().AsSingle();

            sub.BindStateMachine<IStateMachine, ServerStateMachine>();
            
            sub.BindState<InitializeClientsSubstate>();
            sub.BindState<ClientTurnSubstate>();
            sub.BindState<FieldSyncSubstate>();
            sub.BindState<RoundResultSubstate>();

            sub.Bind<FieldModel>()
                .AsSingle()
                .WithArguments(FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            
            sub.BindInterfacesTo<ServerRoundCounter>()
                .AsSingle();
        }
        
        private void BindDebugContracts(DiContainer container)
        {
            container.BindInterfacesTo<ServerStateProviderDebug>()
                .AsSingle();
            container.BindInterfacesTo<ServerStateMachineDebug>()
                .AsSingle()
                .NonLazy();
        }
    }
}