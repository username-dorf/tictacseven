using System;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;

namespace Multiplayer.Server.States
{
    public abstract class ServerSubstate : IState
    {
        protected IStateMachine SubstateMachine { get; }
        
        public ServerSubstate(IServerSubstateResolver substateResolverFactory)
        {
            SubstateMachine = substateResolverFactory.Resolve<IStateMachine>();
        }

        public abstract UniTask EnterAsync(CancellationToken ct);

        public abstract UniTask ExitAsync(CancellationToken ct);

        public abstract void Dispose();
    }
    
    public abstract class ServerSubstate<TPayload> : ServerSubstate, IPayloadedState<TPayload>
    {
        private TPayload _payload;

        protected ServerSubstate(IServerSubstateResolver substateResolverFactory)
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