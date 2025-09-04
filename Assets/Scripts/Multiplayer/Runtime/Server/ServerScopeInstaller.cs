using Core.StateMachine;
using Game.Field;
using Multiplayer.Server.States;
using Zenject;

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
        }

        private void InstallMatchSubcontainer(DiContainer sub)
        {
            sub.BindInterfacesAndSelfTo<ServerSubstateScope>().AsSingle();
            sub.BindInterfacesTo<ServerActiveClientProvider>().AsSingle();
            sub.BindInterfacesTo<ServerClientsProvider>().AsSingle();

            sub.BindInterfacesAndSelfTo<StateFactory>().AsSingle();
            sub.BindInterfacesAndSelfTo<StateMachine>().AsSingle();
            
            sub.InstallState<InitClientsSubstate>();
            sub.InstallState<ClientTurnSubstate>();
            sub.InstallState<FieldSyncSubstate>();
            sub.InstallState<RoundResultSubstate>();

            sub.Bind<FieldModel>()
                .AsSingle()
                .WithArguments(FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
        }
    }
}