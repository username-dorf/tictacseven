using Multiplayer.Lan;
using Multiplayer.UI.Windows.Views;
using Zenject;

namespace Multiplayer
{
    public class MenuMultiplayerMonoInstaller : MonoInstaller<MenuMultiplayerMonoInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<LanClientScanner>()
                .AsSingle();
            Container.BindInterfacesTo<LanHostBroadcaster>()
                .AsSingle();
            Container.Bind<ConnectionController>()
                .AsSingle();
            Container.Bind<LanController>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<JoinApprovalService>()
                .AsSingle();
            Container.Bind<JoinResponseListener>()
                .AsSingle();
            
            Container.Bind<UIWindowCreateHost.ViewModel>().AsTransient();
            Container.Bind<UIWindowConnectClient.ViewModel>().AsTransient();

        }
    }
}