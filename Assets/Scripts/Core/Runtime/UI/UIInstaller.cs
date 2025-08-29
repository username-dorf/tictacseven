using Core.UI.Components;
using Core.UI.Windows;
using Zenject;

namespace Core.UI
{
    public class UIInstaller : Installer<UIInstaller>
    {
        public override void InstallBindings()
        {
            WindowsInstaller.Install(Container);
            
            UIRoundResultFactoryInstaller.Install(Container);
            UIEntitySkinViewInstaller.Install(Container);
        }
    }
}