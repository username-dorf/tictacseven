using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using UniState;
using Zenject;

namespace Core
{
    public class Bootstrap : IInitializable
    {
        private IStateMachine _stateMachine;
        private Config _config;

        public Bootstrap(IStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _config = new Config();
        }
        public void Initialize()
        {
            _config.Initialize();
            _stateMachine.Execute<BootstrapState>(CancellationToken.None)
                .Forget();
        }
    }
}