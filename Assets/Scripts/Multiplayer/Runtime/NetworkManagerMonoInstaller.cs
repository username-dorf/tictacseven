using FishNet.Managing;
using UnityEngine;
using Zenject;

namespace Multiplayer
{
    public class NetworkManagerMonoInstaller : MonoInstaller<NetworkManagerMonoInstaller>
    {
        [Header("FishNet NetworkManager prefab")] [SerializeField]
        private NetworkManager networkManagerPrefab;

        public override void InstallBindings()
        {
            Container
                .Bind<NetworkManager>()
                .FromComponentInNewPrefab(networkManagerPrefab)
                .UnderTransform(ProjectContext.Instance.transform)
                .AsSingle()
                .NonLazy();
        }
    }
}