using System.Linq;
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
            var playerOwner = 1;
            
            var fieldView = await _fieldFactory.CreateAsync(cancellationToken);
            var fieldGrid = _fieldGridFactory.Create(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetPoints(fieldGrid);
            
            var debugMatrixLines = _fieldGridFactory.CreateLines(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetLines(debugMatrixLines);
            
            var entitiesBackgroundView = await _entitiesBackgroundFactory.CreateAsync(cancellationToken);
            var entitiesDebugPositions = _entitiesBackgroundGridFactory.Create(entitiesBackgroundView.Collider, 7);
            entitiesBackgroundView.DebugView.SetPoints(entitiesDebugPositions);

            var entities = await 
                _entityFactory.CreateAll(FieldConfig.ENTITIES_COUNT, entitiesDebugPositions, playerOwner, cancellationToken);
            
            var fieldModel = new FieldModel(fieldGrid,FieldConfig.PRESPAWN_PRESET_1);
            var fieldViewModel = new FieldViewModel(fieldModel, entities
                .Select(x=>x.viewModel));
            
            var existingEntities =
                await _entityFactory.CreateExisting(fieldGrid, FieldConfig.PRESPAWN_PRESET_1, cancellationToken);
        }

        public void Dispose()
        {
            _fieldFactory?.Dispose();
        }
    }
}