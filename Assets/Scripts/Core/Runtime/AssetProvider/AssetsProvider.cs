using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core.AssetProvider
{
    public abstract class AssetsProvider<A, TKey> : IAssetsLoader, IAssetsProvider<A, TKey>, IDisposable
        where A : UnityEngine.Object
    {
        public bool IsLoaded { get; protected set; }

        private readonly Func<A, TKey> _keySelector;
        private readonly Func<TKey, TKey> _requestKeyResolver;
        private readonly bool _returnSingleOnMiss;

        private readonly bool _aIsComponent;
        private AsyncOperationHandle<IList<A>> _handleA;
        private AsyncOperationHandle<IList<GameObject>> _handleGO;
        private Dictionary<TKey, A> _assets;

        protected AssetsProvider(
            Func<A, TKey> keySelector,
            Func<TKey, TKey> requestKeyResolver = null,
            bool returnSingleOnMiss = false)
        {
            _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            _requestKeyResolver = requestKeyResolver ?? (k => k);
            _returnSingleOnMiss = returnSingleOnMiss;
            _aIsComponent = typeof(Component).IsAssignableFrom(typeof(A));
        }

        public virtual async UniTask LoadAssets(CancellationToken ct, params string[] assetKeys)
        {
            if (IsLoaded) return;
            if (assetKeys == null || assetKeys.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(assetKeys), "Keys cannot be empty");

            var keys = assetKeys.Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct()
                .Cast<object>()
                .ToList();
            if (keys.Count == 0) throw new ArgumentOutOfRangeException(nameof(assetKeys), "Keys cannot be empty");

            if (_aIsComponent)
            {
                _handleGO = Addressables.LoadAssetsAsync<GameObject>(keys, null, Addressables.MergeMode.Union);
                await BuildIndexFromGO(ct, _handleGO);
            }
            else
            {
                _handleA = Addressables.LoadAssetsAsync<A>(keys, null, Addressables.MergeMode.Union);
                await BuildIndexFromA(ct, _handleA);
            }
        }

        public async UniTask LoadAssetsByLabels(CancellationToken ct, Addressables.MergeMode mergeMode,
            params string[] labels)
        {
            if (IsLoaded) return;
            if (labels == null || labels.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(labels), "Labels cannot be empty");

            var labs = labels.Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();
            if (labs.Count == 0) throw new ArgumentOutOfRangeException(nameof(labels), "Labels cannot be empty");

            if (_aIsComponent)
            {
                _handleGO = Addressables.LoadAssetsAsync<GameObject>(labs, null, mergeMode);
                await BuildIndexFromGO(ct, _handleGO);
            }
            else
            {
                _handleA = Addressables.LoadAssetsAsync<A>(labs, null, mergeMode);
                await BuildIndexFromA(ct, _handleA);
            }
        }

        public A GetAsset(TKey key)
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Assets not loaded. Call LoadAssets/LoadByLabels() first.");

            key = _requestKeyResolver(key);

            if (_assets != null && _assets.TryGetValue(key, out var val) && val)
                return val;

            if (_returnSingleOnMiss && _assets != null && _assets.Count == 1)
                return _assets.Values.First();

            throw new KeyNotFoundException($"Asset with key '{key}' not found");
        }

        public bool TryGetAsset(TKey key, out A asset)
        {
            asset = null;
            if (!IsLoaded) return false;
            key = _requestKeyResolver(key);
            if (_assets != null && _assets.TryGetValue(key, out var val) && val)
            {
                asset = val;
                return true;
            }

            if (_returnSingleOnMiss && _assets != null && _assets.Count == 1)
            {
                asset = _assets.Values.First();
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            ReleaseAll();
        }

        public void ReleaseAll()
        {
            if (_handleA.IsValid())
                Addressables.Release(_handleA);
            if (_handleGO.IsValid())
                Addressables.Release(_handleGO);

            _handleA = default;
            _handleGO = default;
            _assets = null;
            IsLoaded = false;
        }

        private async UniTask BuildIndexFromA(CancellationToken ct, AsyncOperationHandle<IList<A>> handle)
        {
            try
            {
                await handle.ToUniTask(cancellationToken: ct);
                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null ||
                    handle.Result.Count == 0)
                    throw new Exception("No assets matched the query");

                var dict = new Dictionary<TKey, A>();
                foreach (var a in handle.Result)
                {
                    if (!a) continue;
                    var key = _keySelector(a);
                    if (!dict.TryAdd(key, a))
                        Debug.LogWarning($"Duplicate key '{key}' for asset '{a.name}', skipping");
                }

                if (dict.Count == 0) throw new Exception("No assets produced valid keys");

                _assets = dict;
                IsLoaded = true;
            }
            catch (OperationCanceledException)
            {
                ReleaseAll();
                IsLoaded = false;
                throw;
            }
            catch
            {
                ReleaseAll();
                IsLoaded = false;
                throw;
            }
        }

        private async UniTask BuildIndexFromGO(CancellationToken ct, AsyncOperationHandle<IList<GameObject>> handle)
        {
            try
            {
                await handle.ToUniTask(cancellationToken: ct);
                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null ||
                    handle.Result.Count == 0)
                    throw new Exception("No assets matched the query");

                var typeA = typeof(A);
                var dict = new Dictionary<TKey, A>();
                foreach (var go in handle.Result)
                {
                    if (!go) continue;
                    var comp = go.GetComponent(typeA) as A;
                    if (!comp) continue;

                    var key = _keySelector(comp);
                    if (EqualityComparer<TKey>.Default.Equals(key, default)) continue;

                    if (!dict.TryAdd(key, comp))
                        Debug.LogWarning($"Duplicate key '{key}' for asset '{go.name}', skipping");
                }

                if (dict.Count == 0) throw new Exception("No assets produced valid keys");

                _assets = dict;
                IsLoaded = true;
            }
            catch (OperationCanceledException)
            {
                ReleaseAll();
                IsLoaded = false;
                throw;
            }
            catch
            {
                ReleaseAll();
                IsLoaded = false;
                throw;
            }
        }
    }
}