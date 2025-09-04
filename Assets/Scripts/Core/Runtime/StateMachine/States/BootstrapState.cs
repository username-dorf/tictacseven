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
        
        public async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            await _stateMachine.ChangeStateAsync<GameState>(false);
        }

        public async UniTask ExitAsync(CancellationToken cancellationToken)
        {
        }

    }
}