using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Field
{
    public class FieldViewFactory : IDisposable
    {
        private FieldAssetProvider _assetProvider;
        private FieldViewProvider _fieldViewProvider;

        public FieldViewFactory(FieldViewProvider fieldViewProvider)
        {
            _fieldViewProvider = fieldViewProvider;
            _assetProvider = new FieldAssetProvider();
        }
        
        public async UniTask<FieldView> CreateAsync(CancellationToken cancellationToken)
        {
            var prefab = await _assetProvider.LoadAssetAsync(cancellationToken);
            if(prefab is null)
                return null;
            var view = GameObject.Instantiate(prefab);
            if (view is null)
                throw new Exception("Failed to instantiate FieldView from prefab");
            _fieldViewProvider.Initialize(view);
            return view;
        }
        
        public void Dispose()
        {
            _assetProvider?.Dispose();
        }

        private class FieldAssetProvider : IDisposable
        {
            private AsyncOperationHandle<GameObject> _handle;
            
            private const string AssetPath = "FieldModel_tiled";
            
            public async UniTask<FieldView> LoadAssetAsync(CancellationToken cancellationToken)
            {
                _handle = Addressables.LoadAssetAsync<GameObject>(AssetPath);
                try
                {
                    await _handle.ToUniTask(cancellationToken: cancellationToken);
                    var prefab = _handle.Result;
                    var component = prefab.GetComponent<FieldView>();
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