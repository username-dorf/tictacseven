using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace Game.Entities
{
    public class EntityFactory
    {
        private const string DEFAULT_COLLECTION = "chess";

        private readonly object _loadLock = new();
        private Task<EntityView[]> _loadingTask;
        private EntityView[] _loadedCollectionViews;
        private EntityAssetProvider _assetProvider;
        private DiContainer _diContainer;

        public EntityFactory(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _assetProvider = new EntityAssetProvider();
        }
        public async UniTask<(EntityViewModel viewModel, EntityModel model)[]> CreateAll(int modelsCount,
            Vector3[] positions, CancellationToken cancellationToken)
        {
            var models = new EntityModel[modelsCount];
            for (int i = 0; i < modelsCount; i++)
            {
                models[i] = new EntityModel(i + 1, positions[i]);
            }
            return await CreateAll(models, positions, cancellationToken);
        }

        public async UniTask<(EntityViewModel viewModel, EntityModel model)[]> CreateAll(EntityModel[] models,
            Vector3[] positions, CancellationToken cancellationToken)
        {
            var modelsCount = models.Length;
            var positionsCount = positions.Length;

            if (modelsCount != positionsCount)
                throw new ArgumentException("Models count must be equal to positions count");

            var tasks = new UniTask<(EntityViewModel viewModel, EntityModel model)>[modelsCount];

            for (int i = 0; i < modelsCount; i++)
            {
                tasks[i] = Create(models[i], positions[i], cancellationToken);
            }
            return await UniTask.WhenAll(tasks);
        }
        public async UniTask<(EntityViewModel viewModel, EntityModel model)> Create(EntityModel model, Vector3 position, CancellationToken cancellationToken)
        {
            if (_loadedCollectionViews == null)
            {
                lock (_loadLock)
                {
                    if (_loadingTask is null || _loadingTask.Status != TaskStatus.Running)
                    {
                        _loadingTask = _assetProvider.LoadCollectionAssets(DEFAULT_COLLECTION, cancellationToken);
                    }
                }
                _loadedCollectionViews = await _loadingTask;
            }
            var view = GameObject.Instantiate(_loadedCollectionViews[model.Value.Value-1], position, Quaternion.identity);
            
            //TODO: set base scale for collection
            view.SetScale(0.8f);
            
            var viewModel = _diContainer.Instantiate<EntityViewModel>(new object[]{model});
            view.Initialize(viewModel);
            return (viewModel, model);
        }

        private class EntityAssetProvider : IDisposable
        {
            private const string COLLECTION_PATH_TEMPLATE = "0%value%_%collection%";
            
            private AsyncOperationHandle<GameObject>[] _assetHandles;

            public async Task<EntityView[]> LoadCollectionAssets(string collection, CancellationToken cancellationToken, int length = 7)
            {
                var tasks = new UniTask<(EntityView view, AsyncOperationHandle<GameObject> handle)>[length];

                for (int i = 0; i < length; i++)
                {
                    tasks[i] = LoadAsset(collection, i+1, cancellationToken);
                }

                var results = await UniTask.WhenAll(tasks);
                _assetHandles = results.Select(x => x.handle).ToArray();
                
                if (_assetHandles.Any(handle => !handle.IsValid()))
                    throw new Exception("Failed to load some assets from collection: " + collection);
                if(results.Any(x=>x.view is null))
                    throw new Exception("Failed to load some assets from collection: " + collection);

                return results
                    .Select(tuple => tuple.view)
                    .ToArray();
            }
            
            private async UniTask<(EntityView,AsyncOperationHandle<GameObject>)> LoadAsset(string collection, int index, CancellationToken cancellationToken)
            {
                var path = COLLECTION_PATH_TEMPLATE
                    .Replace("%value%", index.ToString())
                    .Replace("%collection%", collection);
                
                var handle = Addressables.LoadAssetAsync<GameObject>(path);
                try
                {
                    await handle.ToUniTask(cancellationToken: cancellationToken);
                    return (handle.Result.GetComponent<EntityView>(),handle);
                }
                catch (OperationCanceledException)
                {
                    return (null,default);
                }
            }

            public void Dispose()
            {
                if (_assetHandles != null)
                {
                    foreach (var handle in _assetHandles)
                    {
                        if (handle.IsValid())
                        {
                            Addressables.Release(handle);
                        }
                    }
                    _assetHandles = null;
                }
            }
        }
    }
}