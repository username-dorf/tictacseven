using System.Threading;
using Cysharp.Threading.Tasks;
using UniState;


namespace Core.StateMachine
{
    public class BootstrapState : StateBase
    {
        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            return Transition.GoTo<PersistantResourcesLoadState>();
        }
    }
}