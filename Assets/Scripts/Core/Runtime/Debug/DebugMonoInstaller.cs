using Core.AppDebug.Components;
using UnityEngine;
using Zenject;

namespace Core.AppDebug
{
    public class DebugMonoInstaller : MonoInstaller
    {
        [SerializeField] private UIDebugView debugViewPrefab;
        [SerializeField] private UIDebugStringView debugStringViewPrefab;

        public override void InstallBindings()
        {
            Container
                .Bind<UIDebugView>()
                .WithId(AppDebugConfig.UIDebugViewTag)
                .FromComponentInNewPrefab(debugViewPrefab)
                .UnderTransform(ProjectContext.Instance.transform)
                .AsSingle()
                .NonLazy()
                .IfNotBound();;
            
            
            Container.Bind<UIDebugStringView>()
                .WithId(AppDebugConfig.UIDebugStringViewTag)
                .FromInstance(debugStringViewPrefab)
                .AsSingle();
        }
    }
}