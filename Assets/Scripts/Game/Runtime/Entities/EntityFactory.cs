using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;
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
        private FieldViewProvider _fieldViewProvider;

        public EntityFactory(FieldViewProvider fieldViewProvider)
        {
            _fieldViewProvider = fieldViewProvider;
            _assetProvider = new EntityAssetProvider();
        }
        public async UniTask<UserEntitiesModel> CreateAll(Vector3[] positions, int owner, CancellationToken cancellationToken)
        {
            var models = new EntityModel[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                models[i] = new EntityModel(i + 1, owner, positions[i]);
            }
            var entities= await CreateAll(models, cancellationToken);
            var userEntitiesModel = new UserEntitiesModel(entities
                .Select(x => (IPlaceableModel) x.model));
            var userEntitiesViewModel = new UserEntitiesViewModel(userEntitiesModel);
            return userEntitiesModel;
        }

        public async UniTask<UserEntitiesModel> CreateAll(Vector3[] positions, int owner, EntityPlacedModel[] placedModels,
            Vector3[,] gridPositions, CancellationToken cancellationToken)
        {
            if (placedModels is null || placedModels.Length == 0)
            {
                return await CreateAll(positions, owner, cancellationToken);
            }
            
            
            var models = new EntityModel[positions.Length];
            for (var i = 0; i < models.Length; i++)
            {
                if (placedModels.Any(x => x.Data.Merit.Value == i + 1))
                {
                    var placedModel = placedModels.First(x => x.Data.Merit.Value == i + 1);
                    var coors = placedModel.GridPosition.Value;
                    var position = gridPositions[coors.x, coors.y];
                    models[i] = new EntityModel(placedModel.Data.Merit.Value, placedModel.Data.Owner.Value,
                        position);
                    models[i].Transform.SetLocked(true);
                }
                else
                {
                    models[i] = new EntityModel(i + 1, owner, positions[i]);
                }
            }
            var entities = await CreateAll(models, cancellationToken);
            var availableEntities = entities.Where(x => !x.model.Transform.Moveable.Value);
            var userEntitiesModel = new UserEntitiesModel(availableEntities
                .Select(x => (IPlaceableModel) x.model));
            var userEntitiesViewModel = new UserEntitiesViewModel(userEntitiesModel);
            return userEntitiesModel;
        }

        private async UniTask<(EntityViewModel viewModel, EntityModel model)[]> CreateAll(EntityModel[] models,
             CancellationToken cancellationToken)
        {
            var modelsCount = models.Length;
            
            var tasks = new UniTask<(EntityViewModel viewModel, EntityModel model)>[modelsCount];

            for (int i = 0; i < modelsCount; i++)
            {
                tasks[i] = Create(models[i], models[i].Transform.InitialPosition.Value, cancellationToken);
            }
            return await UniTask.WhenAll(tasks);
        }
        private async UniTask<(EntityViewModel viewModel, EntityModel model)> Create(EntityModel model, Vector3 position, CancellationToken cancellationToken)
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
            var view = GameObject.Instantiate(_loadedCollectionViews[model.Data.Merit.Value-1], position, Quaternion.identity);
            
            //TODO: set base scale for collection
            view.SetScale(0.8f);
            
            var viewModel = new EntityViewModel(model);
            view.Initialize(viewModel,_fieldViewProvider);
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