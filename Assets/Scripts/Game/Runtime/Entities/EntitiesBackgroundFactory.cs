using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.User;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Entities
{
    public class EntitiesBackgroundFactory : IDisposable
    {
        private EntitiesBackgroundProvider _assetProvider;
        private EntitiesBackgroundView.EntitiesPlaceholderViewFactory _placeholderViewFactory;
        
        public EntitiesBackgroundFactory(EntitiesValueSpriteProvider spriteProvider)
        {
            _assetProvider = new EntitiesBackgroundProvider();
            _placeholderViewFactory = new EntitiesBackgroundView.EntitiesPlaceholderViewFactory(spriteProvider);
        }
        
        public async UniTask<EntitiesBackgroundView> CreateAsync(CancellationToken cancellationToken)
        {
            var prefab = await _assetProvider.LoadAssetAsync(cancellationToken);
            if(prefab is null)
                return null;
            return GameObject.Instantiate(prefab);
        }
        public async UniTask<EntitiesBackgroundView.EntitiesPlaceholderPresenter> CreatePlaceholdersAsync(UserEntitiesModel userEntitiesModel, CancellationToken cancellationToken)
        {
            return await _placeholderViewFactory.Create(userEntitiesModel, cancellationToken);
        }

        public async UniTask<EntitiesBackgroundView> CreateOpponentAsync(CancellationToken cancellationToken)
        {
            var prefab = await _assetProvider.LoadAssetAsync(cancellationToken);
            if(prefab is null)
                return null;
            var opponentPosition = new Vector3(7.5f, 0, 7.5f) + prefab.transform.position;
            return GameObject.Instantiate(prefab, opponentPosition, Quaternion.identity);
        }
        
        public void Dispose()
        {
            _assetProvider?.Dispose();
        }
        
        private class EntitiesBackgroundProvider : IDisposable
        {
            private AsyncOperationHandle<GameObject> _handle;
            
            private const string AssetPath = "EntitiesBackModel_tiled";
            
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