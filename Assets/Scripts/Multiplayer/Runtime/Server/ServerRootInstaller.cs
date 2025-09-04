using UnityEngine;
using Zenject;

namespace Multiplayer.Server
{
    public class ServerRootInstaller: MonoInstaller
    {
        [SerializeField] private GameObject _serverRootPrefab;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ServerAccessor>()
                .AsSingle();
            
            Container.BindFactory<ServerFacade, ServerFacade.Factory>()
                .FromSubContainerResolve()
                .ByNewContextPrefab(_serverRootPrefab) 
                .UnderTransformGroup("HostRoot");
        }
    }
}