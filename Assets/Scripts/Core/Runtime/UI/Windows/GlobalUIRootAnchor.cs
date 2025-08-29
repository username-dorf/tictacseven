using UnityEngine;
using Zenject;

namespace Core.UI.Windows
{
    public class GlobalUISettings
    {
        public const string GLOBAL_UI_ROOT_ID = "GlobalUIRootPrefab";
    }

    public class GlobalUIInstaller : MonoInstaller
    {
        [SerializeField] private GameObject rootPrefab;
        public override void InstallBindings()
        {
            Container.Bind<GameObject>()
                .WithId(GlobalUISettings.GLOBAL_UI_ROOT_ID)
                .FromInstance(rootPrefab)
                .AsSingle();
            
            Container.BindInterfacesTo<GlobalUIAnchorBootstrap>()
                .AsSingle();
            
        }
    }
    public sealed class GlobalUIAnchorBootstrap : IInitializable
    {
        private readonly IUIRootService _svc;
        private readonly DiContainer _container;
        private readonly GameObject _rootPrefab;

        public GlobalUIAnchorBootstrap(IUIRootService svc, DiContainer container, [Inject(Id=GlobalUISettings.GLOBAL_UI_ROOT_ID)] GameObject prefab)
        {
            _svc = svc; _container = container; _rootPrefab = prefab;
        }

        public void Initialize()
        {
            var go = _container.InstantiatePrefab(_rootPrefab);
            Object.DontDestroyOnLoad(go);
            _svc.SetRoot(go.GetComponent<RectTransform>());
        }
    }
}