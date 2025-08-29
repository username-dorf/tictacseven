using System;
using System.Linq;
using System.Threading;
using Core.AssetProvider;
using Core.Common;
using Core.Data;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;
using UnityEngine;

namespace Game.Entities
{
    public class EntityViewConfig
    {
        private const string DEFAULT_COLLECTION = "chess";
        private const string DEFAULT_VALUE_SPRITE_COLLECTION = "base";
        public string Collection { get; }
        public string SpriteCollection { get; }

        public EntityViewConfig(string collection, string spriteCollection)
        {
            Collection = collection;
            SpriteCollection = spriteCollection;
        }

        public static EntityViewConfig Default()
        {
            return new EntityViewConfig(DEFAULT_COLLECTION, DEFAULT_VALUE_SPRITE_COLLECTION);
        }
    }

    public class EntityFactory :IDisposable
    {
        private AssetsProvider<MaterialApplicableView, int> _assetsLoader;
        private FieldViewProvider _fieldViewProvider;
        private EntitiesValueSpriteProvider _valueSpriteProvider;
        private EntitiesMaterialAssetsProvider _materialAssetsProvider;

        public EntityFactory(
            FieldViewProvider fieldViewProvider,
            EntitiesValueSpriteProvider valueSpriteProvider,
            EntitiesMaterialAssetsProvider materialAssetsProvider)
        {
            _materialAssetsProvider = materialAssetsProvider;
            _valueSpriteProvider = valueSpriteProvider;
            _fieldViewProvider = fieldViewProvider;
            _assetsLoader = new EntitySingleAssetsProvider();
        }

        public async UniTask<UserEntitiesModel> CreateAll(Vector3[] positions, int owner,
            CancellationToken cancellationToken)
        {
            await WarmupAllAsync(EntityViewConfig.Default(), cancellationToken);

            var models = new EntityModel[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                models[i] = new EntityModel(i + 1, owner, positions[i]);
            }

            var entities = await CreateAll(models, cancellationToken);
            var userEntitiesModel = new UserEntitiesModel(entities
                .Select(x => (IPlaceableModel) x.model));
            var userEntitiesViewModel = new UserEntitiesViewModel(userEntitiesModel);
            return userEntitiesModel;
        }

        public async UniTask<UserEntitiesModel> CreateAll(Vector3[] positions, int owner,
            EntityPlacedModel[] placedModels,
            Vector3[,] gridPositions, CancellationToken cancellationToken)
        {
            if (placedModels is null || placedModels.Length == 0)
            {
                return await CreateAll(positions, owner, cancellationToken);
            }

            await WarmupAllAsync(EntityViewConfig.Default(), cancellationToken);

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
                    models[i].Transform.SetMoveable(false);
                }
                else
                {
                    models[i] = new EntityModel(i + 1, owner, positions[i]);
                }
            }

            var entities = await CreateAll(models, cancellationToken);
            var availableEntities = entities.Where(x => !x.model.Transform.IsMoveable.Value);
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

        private async UniTask<(EntityViewModel viewModel, EntityModel model)> Create(EntityModel model,
            Vector3 position, CancellationToken cancellationToken)
        {
            var viewPrefab = _assetsLoader.GetAsset(model.Data.Merit.Value);
            var view = (EntityView)GameObject.Instantiate(viewPrefab, position,
                Quaternion.identity);
            
            var materialId = model.Data.Owner.Value == 2 ? MaterialId.Default : MaterialId.Opponent;
            var material = _materialAssetsProvider.Get(materialId);
            var valueSprite = _valueSpriteProvider.GetAsset(model.Data.Merit.Value);
            var viewModel = new EntityViewModel(model, valueSprite, material);
            view.Initialize(viewModel, _fieldViewProvider);
            return (viewModel, model);
        }

        private async UniTask WarmupAllAsync(EntityViewConfig config, CancellationToken ct)
        {
            var materialsTask = _materialAssetsProvider.LoadAll(ct, config.SpriteCollection);
            var spritesTask = _valueSpriteProvider.LoadAll(ct, config.SpriteCollection);
            var assetsTask = _assetsLoader.LoadAssets(ct, config.Collection);

            await UniTask.WhenAll(materialsTask, assetsTask, spritesTask);
        }

        

        public sealed class EntityCollectionAssetsLoader : AssetsProvider<EntityView, int>
        {
            private const string TEMPLATE = "0{0}_{1}";
            private const int LENGTH = 7;

            public EntityCollectionAssetsLoader()
                : base(SelectKey)
            {
            }

            private static int SelectKey(EntityView go)
            {
                var n = go ? go.name : null;
                if (string.IsNullOrEmpty(n)) return 0;

                int v = 0;
                bool any = false;
                for (int i = 0; i < n.Length; i++)
                {
                    char c = n[i];
                    if (char.IsDigit(c))
                    {
                        any = true;
                        v = v * 10 + (c - '0');
                    }
                    else
                    {
                        if (any) break;
                        if (c == '_') break;
                    }
                }

                return any ? v : 0;
            }

            public override async UniTask LoadAssets(CancellationToken ct, params string[] assetKeys)
            {
                if (IsLoaded) return;
                if (assetKeys == null || assetKeys.Length == 0)
                    throw new ArgumentOutOfRangeException(nameof(assetKeys), "Collection name required");

                var collection = assetKeys[0];
                var keys = Enumerable.Range(1, LENGTH)
                    .Select(i => string.Format(TEMPLATE, i, collection))
                    .ToArray();

                await base.LoadAssets(ct, keys);
            }
        }

        public void Dispose()
        {
            _assetsLoader?.Dispose();
            _valueSpriteProvider?.Dispose();
            _materialAssetsProvider?.Dispose();
        }
    }
}