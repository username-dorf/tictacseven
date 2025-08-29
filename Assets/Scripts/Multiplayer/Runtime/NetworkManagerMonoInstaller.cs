using FishNet.Managing;
using FishNet.Managing.Object;
using UnityEngine;
using Zenject;

namespace Multiplayer
{
    public class NetworkManagerMonoInstaller : MonoInstaller<NetworkManagerMonoInstaller>
    {
        [Header("FishNet NetworkManager prefab")]
        [SerializeField] private NetworkManager networkManagerPrefab;
        
        public override void InstallBindings()
        {
            var networkManager = Container.InstantiatePrefabForComponent<NetworkManager>(
                networkManagerPrefab, ProjectContext.Instance.transform);
            Container.Bind<NetworkManager>().FromInstance(networkManager).AsSingle().NonLazy();
        }
    }
}