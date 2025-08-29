using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public class BootstrapState : IState
    {
        private IStateMachine _stateMachine;

        public BootstrapState(IStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await _stateMachine.ChangeStateAsync<PersistantResourcesLoadState>(ct);
        }

        public async UniTask ExitAsync(CancellationToken cancellationToken)
        {
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}