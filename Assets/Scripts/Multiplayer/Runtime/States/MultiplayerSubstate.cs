using System;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;

namespace Multiplayer.States
{
    public abstract class MultiplayerSubstate : IState
    {
        protected IStateMachine SubstateMachine { get; }
        
        public MultiplayerSubstate(IMultiplayerSubstateResolver substateResolverFactory)
        {
            SubstateMachine = substateResolverFactory.Resolve<IStateMachine>();
        }

        public abstract UniTask EnterAsync(CancellationToken ct);

        public abstract UniTask ExitAsync(CancellationToken cancellationToken);

        public abstract void Dispose();
    }
    
    public abstract class MultiplayerSubstate<TPayload> : MultiplayerSubstate, IPayloadedState<TPayload>
    {
        private TPayload _payload;

        protected MultiplayerSubstate(IMultiplayerSubstateResolver substateResolverFactory)
            : base(substateResolverFactory) { }

        public sealed override UniTask EnterAsync(CancellationToken ct)
        {
            if (_payload is null)
                throw new InvalidOperationException(
                    $"{GetType().Name} Required payload" +
                    $"Call ChangeStateAsync<{GetType().Name}, {typeof(TPayload).Name}>(payload).");

            return EnterAsync(_payload, ct);
        }

        void IPayloadedState<TPayload>.SetPayload(TPayload payload)
        {
            _payload = payload;
        }
        protected abstract UniTask EnterAsync(TPayload payload, CancellationToken ct);
    }
}