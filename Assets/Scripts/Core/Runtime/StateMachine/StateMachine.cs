using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public interface IStateMachine
    {
        UniTask ChangeStateAsync<T>() where T : IState;
        UniTask ChangeStateAsync<T>(bool awaitExiting) where T : IState;
    }


    public class StateMachine : IStateMachine, IDisposable
    {
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

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}