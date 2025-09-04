using Multiplayer.Client;
using Multiplayer.Connection;
using Multiplayer.Server;
using Multiplayer.UI.Windows.Views;
using Zenject;

namespace Multiplayer
{
    public class MenuMultiplayerMonoInstaller : MonoInstaller<MenuMultiplayerMonoInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ClientScanner>()
                .AsSingle();
            Container.BindInterfacesTo<HostBroadcaster>()
                .AsSingle();
            Container.Bind<ConnectionController>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<SessionController>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<JoinApprovalService>()
                .AsSingle();
            Container.Bind<JoinResponseListener>()
                .AsSingle();
            Container.BindInterfacesTo<HostConnectionProvider>()
                .AsSingle();
            
            Container.Bind<UIWindowCreateHost.ViewModel>().AsTransient();
            Container.Bind<UIWindowConnectClient.ViewModel>().AsTransient();

            Container.BindInterfacesTo<ServerSessionBootstrap>()
                .AsSingle();

            Container.Bind<HostBootstrapper>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<ClientSessionBootstrap>()
                .AsSingle();

        }
    }
}