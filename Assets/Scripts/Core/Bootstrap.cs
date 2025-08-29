using System.Threading;
using Core.StateMachine;
using Zenject;

namespace Core
{
    public class Bootstrap : IInitializable
    {
        private IStateMachine _stateMachine;

        public Bootstrap(IStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }
        public void Initialize()
        {
            _stateMachine.ChangeStateAsync<BootstrapState>(CancellationToken.None);
        }
    }
}