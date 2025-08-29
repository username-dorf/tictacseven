using Core.UI.Windows.Views;
using Zenject;

namespace Core.UI.Windows
{
    public class WindowsInstaller : Installer<WindowsInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<UIRootService>()
                .AsSingle();
            
            Container.BindInterfacesAndSelfTo<WindowsAssetsProvider>()
                .AsSingle();
            
            Container.Bind<IWindowsController>()
                .To<WindowsController>()
                .AsSingle();

            BindWindow();
        }

        private void BindWindow()
        {
            Container.Bind<UIWindowRoundResult.ViewModel>().AsTransient();
            Container.Bind<UIWindowProfileSettings.ViewModel>().AsTransient();
            Container.Bind<UIWindowSettings.ViewModel>().AsTransient();
        }
    }
}