using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.StateMachine
{
    public interface IStateMachine
    {
        IState CurrentState { get; }

        UniTask ChangeStateAsync<T>(CancellationToken ct) where T : IState;
        UniTask ChangeStateAsync<T>(bool awaitExiting, CancellationToken ct) where T : IState;

        UniTask ChangeStateAsync<T>(T state, CancellationToken ct) where T : IState;
        UniTask ChangeStateAsync<T>(T state, bool awaitExiting, CancellationToken ct) where T : IState;

        UniTask ChangeStateAsync<TState, TPayload>(TPayload payload, CancellationToken ct)
            where TState : IState, IPayloadedState<TPayload>;

        UniTask ChangeStateAsync<TState, TPayload>(TPayload payload, bool awaitExiting, CancellationToken ct)
            where TState : IState, IPayloadedState<TPayload>;

        UniTask ChangeStateAsync<TState, TPayload>(TState state, TPayload payload, CancellationToken ct)
            where TState : IState, IPayloadedState<TPayload>;

        UniTask ChangeStateAsync<TState, TPayload>(TState state, TPayload payload, bool awaitExiting,
            CancellationToken ct)
            where TState : IState, IPayloadedState<TPayload>;
    }

    public class StateMachine : IStateMachine, IDisposable
    {
        public IState CurrentState => _currentState;

        private IState _currentState;
        private readonly StateFactory _stateFactory;

        private readonly CancellationTokenSource _lifetimeCts;

        private readonly ConcurrentQueue<QueuedTransition> _stateTransitionQueue;

        private volatile bool _isProcessing;

        private readonly struct QueuedTransition
        {
            public readonly Func<CancellationToken, UniTask> Run;
            public readonly CancellationToken ExternalToken;

            public QueuedTransition(Func<CancellationToken, UniTask> run, CancellationToken externalToken)
            {
                Run = run;
                ExternalToken = externalToken;
            }
        }

        public StateMachine(StateFactory stateFactory)
        {
            _stateFactory = stateFactory;
            _lifetimeCts = new CancellationTokenSource();
            _stateTransitionQueue = new ConcurrentQueue<QueuedTransition>();
            _isProcessing = false;
        }


        public UniTask ChangeStateAsync<T>(CancellationToken ct) where T : IState =>
            ChangeStateAsync<T>(awaitExiting: true, ct);

        public UniTask ChangeStateAsync<T>(bool awaitExiting, CancellationToken ct) where T : IState
        {
            var state = _stateFactory.Get<T>();
            return ChangeStateAsync((T) state, awaitExiting, ct);
        }

        public UniTask ChangeStateAsync<T>(T state, CancellationToken ct) where T : IState =>
            ChangeStateAsync(state, awaitExiting: true, ct);

        public async UniTask ChangeStateAsync<T>(T state, bool awaitExiting, CancellationToken ct) where T : IState
        {
            EnqueueTransition(
                run: token => ChangeStateCoreAsync(state, awaitExiting, token),
                externalCt: ct
            );

            if (!_isProcessing)
                await ProcessStateQueueAsync();
        }


        public UniTask ChangeStateAsync<TState, TPayload>(TPayload payload, CancellationToken ct)
            where TState : IState, IPayloadedState<TPayload> =>
            ChangeStateAsync<TState, TPayload>(payload, awaitExiting: true, ct);

        public UniTask ChangeStateAsync<TState, TPayload>(TPayload payload, bool awaitExiting, CancellationToken ct)
            where TState : IState, IPayloadedState<TPayload>
        {
            var state = (IPayloadedState<TPayload>) _stateFactory.Get<TState>();
            state.SetPayload(payload);
            return ChangeStateAsync((TState) state, payload, awaitExiting, ct);
        }

        public UniTask ChangeStateAsync<TState, TPayload>(TState state, TPayload payload, CancellationToken ct)
            where TState : IState, IPayloadedState<TPayload> =>
            ChangeStateAsync(state, payload, awaitExiting: true, ct);

        public async UniTask ChangeStateAsync<TState, TPayload>(TState state, TPayload payload, bool awaitExiting,
            CancellationToken ct)
            where TState : IState, IPayloadedState<TPayload>
        {
            state.SetPayload(payload);

            EnqueueTransition(
                run: token => ChangeStateCoreAsync(state, awaitExiting, token),
                externalCt: ct
            );

            if (!_isProcessing)
                await ProcessStateQueueAsync();
        }


        private void EnqueueTransition(Func<CancellationToken, UniTask> run, CancellationToken externalCt)
        {
            _stateTransitionQueue.Enqueue(new QueuedTransition(run, externalCt));
        }

        private async UniTask ProcessStateQueueAsync()
        {
            if (_isProcessing) return;

            _isProcessing = true;
            try
            {
                while (_stateTransitionQueue.TryDequeue(out var queued))
                {
                    if (queued.ExternalToken.IsCancellationRequested)
                        continue;

                    using var linkedCts =
                        CancellationTokenSource.CreateLinkedTokenSource(_lifetimeCts.Token, queued.ExternalToken);
                    var token = linkedCts.Token;

                    try
                    {
                        await queued.Run(token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async UniTask ChangeStateCoreAsync(IState nextState, bool awaitExiting, CancellationToken ct)
        {
            if (_currentState != null)
            {
                if (awaitExiting)
                {
                    await _currentState.ExitAsync(ct);
                }
                else
                {
                    _currentState.ExitAsync(ct).Forget();
                }
            }

            _currentState = nextState;

            if (_currentState != null)
            {
                await _currentState.EnterAsync(ct);
            }
        }

        public void Dispose()
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();

            while (_stateTransitionQueue.TryDequeue(out _))
            {
            }
        }
    }
}