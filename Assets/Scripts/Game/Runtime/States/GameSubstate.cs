using System;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;

namespace Game.States
{
    public abstract class GameSubstate : IState
    {
        protected IStateMachine SubstateMachine { get; }
        
        public GameSubstate(IGameSubstateResolver substateResolverFactory)
        {
            SubstateMachine = substateResolverFactory.Resolve<IStateMachine>();
        }

        public abstract UniTask EnterAsync(CancellationToken cancellationToken);

        public abstract UniTask ExitAsync(CancellationToken cancellationToken);
    }
    
    public abstract class GameSubstate<TPayload> : GameSubstate, IPayloadedState<TPayload>
    {
        private TPayload _payload;

        protected GameSubstate(IGameSubstateResolver substateResolverFactory)
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