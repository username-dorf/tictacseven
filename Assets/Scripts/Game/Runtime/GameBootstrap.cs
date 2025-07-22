using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.Entities;
using Game.Field;

namespace Game
{
    public class GameBootstrap : IGameBootstrapAsync
    {
        private readonly FieldFactory _fieldFactory;
        private readonly FieldGridFactory _fieldGridFactory;
        private readonly EntitiesBackgroundFactory _entitiesBackgroundFactory;
        private readonly EntitiesBackgroundGridFactory _entitiesBackgroundGridFactory;
        private readonly EntityFactory _entityFactory;

        public GameBootstrap(
            FieldFactory fieldFactory,
            FieldGridFactory fieldGridFactory,
            EntitiesBackgroundFactory entitiesBackgroundFactory,
            EntitiesBackgroundGridFactory entitiesBackgroundGridFactory,
            EntityFactory entityFactory)
        {
            _entityFactory = entityFactory;
            _entitiesBackgroundGridFactory = entitiesBackgroundGridFactory;
            _entitiesBackgroundFactory = entitiesBackgroundFactory;
            _fieldGridFactory = fieldGridFactory;
            _fieldFactory = fieldFactory;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            var fieldView = await _fieldFactory.CreateAsync(cancellationToken);
            var debugMatrix = _fieldGridFactory.Create(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetPoints(debugMatrix);
            
            var debugMatrixLines = _fieldGridFactory.CreateLines(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetLines(debugMatrixLines);
            
            var fieldModel = new FieldModel(FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);


            var entitiesBackgroundView = await _entitiesBackgroundFactory.CreateAsync(cancellationToken);
            var entitiesDebugPositions = _entitiesBackgroundGridFactory.Create(entitiesBackgroundView.Collider, 7);
            entitiesBackgroundView.DebugView.SetPoints(entitiesDebugPositions);

            var entities = await 
                _entityFactory.CreateAll(FieldConfig.ENTITIES_COUNT, entitiesDebugPositions, cancellationToken);
        }

        public void Dispose()
        {
            _fieldFactory?.Dispose();
        }
    }
}