using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.States;

namespace Game
{
    public class GameBootstrap : IGameBootstrapAsync
    {
        
        private IGameSubstateResolver _gameSubstateResolver;

        public GameBootstrap(
            IGameSubstateResolver gameSubstateResolver)
        {
            _gameSubstateResolver = gameSubstateResolver;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            var substateMachine = _gameSubstateResolver.Resolve<IStateMachine>();
            await substateMachine.ChangeStateAsync<InitialSubstate>();
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}