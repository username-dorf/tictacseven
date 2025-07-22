using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Entities
{
    public class EntitiesBackgroundFactory : IDisposable
    {
        private EntitiesBackgroundProvider _assetProvider;
        
        public EntitiesBackgroundFactory()
        {
            _assetProvider = new EntitiesBackgroundProvider();
        }
        
        public async UniTask<EntitiesBackgroundView> CreateAsync(CancellationToken cancellationToken)
        {
            var prefab = await _assetProvider.LoadAssetAsync(cancellationToken);
            if(prefab is null)
                return null;
            return GameObject.Instantiate(prefab);
        }
        
        public void Dispose()
        {
            _assetProvider?.Dispose();
        }
        
        private class EntitiesBackgroundProvider : IDisposable
        {
            private AsyncOperationHandle<GameObject> _handle;
            
            private const string AssetPath = "EntitiesBackModel";
            
            public async UniTask<EntitiesBackgroundView> LoadAssetAsync(CancellationToken cancellationToken)
            {
                _handle = Addressables.LoadAssetAsync<GameObject>(AssetPath);
                try
                {
                    await _handle.ToUniTask(cancellationToken: cancellationToken);
                    var prefab = _handle.Result;
                    var component = prefab.GetComponent<EntitiesBackgroundView>();
                    if (prefab is null || component is null)
                        throw new Exception($"Failed to load asset at {AssetPath}");
                    return component;
                }
                catch(OperationCanceledException e)
                {
                    return null;
                }
            } 
            public void Dispose()
            {
                if (_handle.IsValid())
                {
                    Addressables.Release(_handle);
                }
            }
        }
    }
}