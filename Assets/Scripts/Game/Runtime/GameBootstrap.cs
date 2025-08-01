using System;
using System.Linq;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.Entities;
using Game.Field;
using Game.User;
using UnityEngine;

namespace Game
{
    public class GameBootstrap : IGameBootstrapAsync
    {
        private readonly FieldViewFactory _fieldViewFactory;
        private readonly FieldGridFactory _fieldGridFactory;
        private readonly EntitiesBackgroundFactory _entitiesBackgroundFactory;
        private readonly EntitiesBackgroundGridFactory _entitiesBackgroundGridFactory;
        private readonly EntityFactory _entityFactory;

        public GameBootstrap(
            FieldViewFactory fieldViewFactory,
            FieldGridFactory fieldGridFactory,
            EntitiesBackgroundFactory entitiesBackgroundFactory,
            EntitiesBackgroundGridFactory entitiesBackgroundGridFactory,
            EntityFactory entityFactory)
        {
            _entityFactory = entityFactory;
            _entitiesBackgroundGridFactory = entitiesBackgroundGridFactory;
            _entitiesBackgroundFactory = entitiesBackgroundFactory;
            _fieldGridFactory = fieldGridFactory;
            _fieldViewFactory = fieldViewFactory;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            var playerOwner = 1;
            var opponentOwner = 2;
            
            var fieldView = await _fieldViewFactory.CreateAsync(cancellationToken);
            var fieldGrid = _fieldGridFactory.Create(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetPoints(fieldGrid);
            
            var debugMatrixLines = _fieldGridFactory.CreateLines(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetLines(debugMatrixLines);
            
            var entitiesBackgroundView = await _entitiesBackgroundFactory.CreateAsync(cancellationToken);
            var entitiesPositions = _entitiesBackgroundGridFactory.Create(entitiesBackgroundView.Collider, FieldConfig.ENTITIES_COUNT);
            entitiesBackgroundView.DebugView.SetPoints(entitiesPositions);

            var userEntitiesModel = await 
                _entityFactory.CreateAll(entitiesPositions, playerOwner, cancellationToken);

            EntityPlacedModel[] prespawnPreset = null;//FieldConfig.CREATE_PRESPAWN_PRESET_1(opponentOwner);
            
            var opponentEntitiesBackgroundView = await _entitiesBackgroundFactory.CreateOpponentAsync(cancellationToken);
            var opponentEntitiesPositions =
                _entitiesBackgroundGridFactory.Create(opponentEntitiesBackgroundView.Collider, 7);
            var opponentEntitiesModel =
                await _entityFactory.CreateAll(opponentEntitiesPositions,opponentOwner, prespawnPreset,
                    fieldGrid, cancellationToken);
            
            var fieldModel = new FieldModel(fieldGrid,prespawnPreset);
            var fieldViewModel = new FieldViewModel(fieldModel, userEntitiesModel, opponentEntitiesModel);


            await UniTask.Delay(TimeSpan.FromSeconds(3),cancellationToken: cancellationToken);
            var controller = new UserEntitiesController(opponentEntitiesModel, fieldModel);
            await controller.DoMoveAsync(3, new Vector2Int(1, 1), cancellationToken);

        }

        public void Dispose()
        {
            _fieldViewFactory?.Dispose();
        }
    }
}