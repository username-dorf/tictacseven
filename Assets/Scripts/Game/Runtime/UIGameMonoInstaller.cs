using Core.UI.Components;
using Game.UI;
using Zenject;

namespace Game
{
    public class UIGameMonoInstaller :MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<UIProvider<UIGame>>()
                .FromComponentInHierarchy()
                .AsSingle();
        }
    }
}