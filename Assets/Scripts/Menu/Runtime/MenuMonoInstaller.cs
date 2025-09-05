using System;
using System.Threading;
using Core.UI;
using Core.UI.Components;
using Cysharp.Threading.Tasks;
using Menu.Runtime.UI;
using Menu.Runtime.UIWorld;
using Menu.UI;
using UnityEngine;
using Zenject;

public class MenuMonoInstaller : MonoInstaller<MenuMonoInstaller>
{
    [SerializeField] private UIProvider<UIMenu> uiMenu; 
    public override void InstallBindings()
    {
        BindMenuComponents(Container);
        
        Container.Bind<UIProvider<UIMenu>>()
            .FromInstance(uiMenu)
            .AsSingle();

        Container.BindInterfacesAndSelfTo<UIMenuController>()
            .AsSingle();
    }

    public void BindMenuComponents(DiContainer container)
    {
        container.BindFactory<Func<CancellationToken, UniTask>, ModeButtonViewModel, ModeButtonViewModel.Factory>()
            .AsSingle();
        container.BindInterfacesAndSelfTo<MenuFieldViewFactory>()
            .AsSingle();
        container.BindInterfacesAndSelfTo<MenuFieldSubviewFactory>()
            .AsSingle();

        container.BindInterfacesTo<MenuBootstrap>()
            .AsSingle();
    }
}
