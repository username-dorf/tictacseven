using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.Field;

namespace Game
{
    public class GameBootstrap : IGameBootstrapAsync
    {
        private readonly FieldFactory _fieldFactory;
        private readonly FieldGridFactory _fieldGridFactory;

        public GameBootstrap(FieldFactory fieldFactory, FieldGridFactory fieldGridFactory)
        {
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
        }

        public void Dispose()
        {
            _fieldFactory?.Dispose();
        }
    }
}