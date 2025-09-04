using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public interface IStateMachine
    {
        IState CurrentState { get; }
        UniTask ChangeStateAsync<T>() where T : IState;
        UniTask ChangeStateAsync<T>(bool awaitExiting) where T : IState;
        
        UniTask ChangeStateAsync<T>(T state) where T : IState;
        UniTask ChangeStateAsync<T>(T state, bool awaitExiting) where T : IState;
        
        
        UniTask ChangeStateAsync<TState, TPayload>(TPayload payload) 
            where TState : IState, IPayloadedState<TPayload>;

        UniTask ChangeStateAsync<TState, TPayload>(TPayload payload, bool awaitExiting) 
            where TState : IState, IPayloadedState<TPayload>;

        UniTask ChangeStateAsync<TState, TPayload>(TState state, TPayload payload) 
            where TState : IState, IPayloadedState<TPayload>;

        UniTask ChangeStateAsync<TState, TPayload>(TState state, TPayload payload, bool awaitExiting) 
            where TState : IState, IPayloadedState<TPayload>;
    }


    public class StateMachine : IStateMachine, IDisposable
    {
        public IState CurrentState => _currentState;
        
        private IState _currentState;
        private readonly StateFactory _stateFactory;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentQueue<Func<UniTask>> _stateTransitionQueue;
        private bool _isTransitioning;

        public StateMachine(StateFactory stateFactory)
        {
            _stateFactory = stateFactory;
            _cancellationTokenSource = new CancellationTokenSource();
            _stateTransitionQueue = new ConcurrentQueue<Func<UniTask>>();
            _isTransitioning = false;
        }
        
        public async UniTask ChangeStateAsync<T>() where T : IState
        {
            await ChangeStateAsync<T>(true);
        }

        public async UniTask ChangeStateAsync<T>(bool awaitExiting) where T : IState
        {
            var state = _stateFactory.Create(typeof(T).Name);
            var transitionTask = new Func<UniTask>(async () =>
            {
                await ChangeStateAsync(state,awaitExiting);
            });

            _stateTransitionQueue.Enqueue(transitionTask);

            if (!_isTransitioning)
            {
                await ProcessStateQueue();
            }
        }

        public UniTask ChangeStateAsync<T>(T state) where T : IState
        {
            return ChangeStateAsync(state, true);
        }

        public async UniTask ChangeStateAsync<T>(T state, bool awaitExiting) where T : IState
        {
            var transitionTask = new Func<UniTask>(async () =>
            {
                await ChangeStateAsync(state,awaitExiting);
            });

            _stateTransitionQueue.Enqueue(transitionTask);

            if (!_isTransitioning)
            {
                await ProcessStateQueue();
            }
        }

        private async UniTask ProcessStateQueue()
        {
            while (_stateTransitionQueue.TryDequeue(out var transitionTask))
            {
                _isTransitioning = true;
                await transitionTask();
                _isTransitioning = false;
            }
        }

        private async UniTask ChangeStateAsync(IState state, bool awaitExiting)
        {
            try
            {
                if (_currentState != null)
                {
                    if (awaitExiting)
                    {
                        await _currentState.ExitAsync(_cancellationTokenSource.Token);
                    }
                    else
                    {
                        _currentState.ExitAsync(_cancellationTokenSource.Token).Forget();
                    }
                }

                _currentState = state;

                if (_currentState != null)
                {
                    await _currentState.EnterAsync(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException e)
            {
            }
        }
        
        public UniTask ChangeStateAsync<TState, TPayload>(TPayload payload)
            where TState : IState, IPayloadedState<TPayload>
        {
            return ChangeStateAsync<TState, TPayload>(payload, awaitExiting: true);
        }

        public async UniTask ChangeStateAsync<TState, TPayload>(TPayload payload, bool awaitExiting)
            where TState : IState, IPayloadedState<TPayload>
        {
            var state = _stateFactory.Create(typeof(TState).Name);

            var payloaded = (IPayloadedState<TPayload>)state;
            payloaded.SetPayload(payload);

            var transitionTask = new Func<UniTask>(async () =>
            {
                await ChangeStateAsync(state, awaitExiting);
            });

            _stateTransitionQueue.Enqueue(transitionTask);
            if (!_isTransitioning)
                await ProcessStateQueue();
        }
        public UniTask ChangeStateAsync<TState, TPayload>(TState state, TPayload payload)
            where TState : IState, IPayloadedState<TPayload>
        {
            return ChangeStateAsync(state, payload, awaitExiting: true);
        }

        public async UniTask ChangeStateAsync<TState, TPayload>(TState state, TPayload payload, bool awaitExiting)
            where TState : IState, IPayloadedState<TPayload>
        {
            state.SetPayload(payload);

            var transitionTask = new Func<UniTask>(async () =>
            {
                await ChangeStateAsync(state, awaitExiting);
            });

            _stateTransitionQueue.Enqueue(transitionTask);
            if (!_isTransitioning)
                await ProcessStateQueue();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}