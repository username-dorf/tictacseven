using Core.UI.Components;
using Game.UI;
using Zenject;

namespace Multiplayer
{
    public class UIMultiplayerGameMonoInstaller: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<UIProvider<UIGame>>()
                .FromComponentInHierarchy()
                .AsSingle();
        }
    }
}