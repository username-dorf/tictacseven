using System.Threading;
using Cysharp.Threading.Tasks;
using UniState;

namespace Core.StateMachine
{
    public interface IStateTrigger
    {
    }

    public interface IManualStateTrigger : IStateTrigger
    {
        void Continue();
        UniTask WhenArrivedAsync(CancellationToken ct);
        void Reset();
        void MarkArrived();
    }

    public abstract class ManualTransitionTriggerBase : IManualStateTrigger
    {
        private UniTaskCompletionSource _firedTcs = new();
        private UniTaskCompletionSource _arrivedTcs = new();

        public void Continue() => _firedTcs.TrySetResult();

        public async UniTask<StateTransitionInfo> WaitAndBuildTransitionAsync(
            IStateTransitionFacade facade, CancellationToken ct)
        {
            await _firedTcs.Task.AttachExternalCancellation(ct);
            return BuildTransition(facade);
        }

        public UniTask WhenArrivedAsync(CancellationToken ct)
            => _arrivedTcs.Task.AttachExternalCancellation(ct);

        public void MarkArrived() => _arrivedTcs.TrySetResult();

        public void Reset()
        {
            if (_firedTcs.Task.Status != UniTaskStatus.Pending) _firedTcs = new();
            if (_arrivedTcs.Task.Status != UniTaskStatus.Pending) _arrivedTcs = new();
        }

        protected abstract StateTransitionInfo BuildTransition(IStateTransitionFacade facade);
    }

    public class ManualTransitionTrigger<T> : ManualTransitionTriggerBase
        where T : AddressablesSceneStateBase
    {
        protected override StateTransitionInfo BuildTransition(IStateTransitionFacade f) => f.GoTo<T>();
    }
}