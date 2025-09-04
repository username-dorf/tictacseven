using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core.AssetProvider
{
    public abstract class AssetProvider<T> : IAssetLoader, IAssetProvider<T> where T : MonoBehaviour
    {
        public bool IsLoaded { get; protected set; }

        protected AsyncOperationHandle<GameObject> _assetHandle;
        protected GameObject _prefab;
        protected T _loaded;
        protected string _loadedKey;

        public virtual async UniTask LoadAsset(CancellationToken cancellationToken, string assetKey)
        {
            
            if (IsLoaded && string.Equals(_loadedKey, assetKey, StringComparison.Ordinal))
                return;

            if (IsLoaded && !string.Equals(_loadedKey, assetKey, StringComparison.Ordinal))
                ReleaseHandle(_assetHandle);

            _assetHandle = Addressables.LoadAssetAsync<GameObject>(assetKey);

            try
            {
                await _assetHandle.ToUniTask(cancellationToken: cancellationToken);

                if (_assetHandle.Status != AsyncOperationStatus.Succeeded || _assetHandle.Result == null)
                {
                    ReleaseHandle(_assetHandle);
                    throw new Exception($"Failed to load asset: {assetKey}");
                }

                _prefab = _assetHandle.Result;
                _loaded = _prefab.GetComponent<T>();
                if (_loaded == null)
                {
                    ReleaseHandle(_assetHandle);
                    throw new Exception($"Component {typeof(T).Name} not found on asset: {assetKey}");
                }

                _loadedKey = assetKey;
                IsLoaded = true;
            }
            catch (OperationCanceledException)
            {
                ReleaseHandle(_assetHandle);
                IsLoaded = false;
                throw;
            }
            catch
            {
                ReleaseHandle(_assetHandle);
                IsLoaded = false;
                throw;
            }
        }

        public T GetAsset()
        {
            if (!IsLoaded) throw new InvalidOperationException("Asset not loaded");
            if (_loaded != null) return _loaded;
            throw new Exception($"Asset not found: {_loadedKey}");
        }

        public void Dispose()
        {
            ReleaseHandle(_assetHandle);
            _prefab = null;
            _loaded = null;
            _loadedKey = null;
            IsLoaded = false;
        }

        protected void ReleaseHandle(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
                if (handle.Equals(_assetHandle)) _assetHandle = default;
            }
        }
    }
}
