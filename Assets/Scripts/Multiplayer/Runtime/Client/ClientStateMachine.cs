using System;
using System.Threading;
using UniState;
using UnityEngine;

namespace Multiplayer.Client
{
    public class ClientStateMachineDisposable : IDisposable
    {
        public CancellationToken Token => _cancellationTokenSource.Token;
        private CancellationTokenSource _cancellationTokenSource;

        public ClientStateMachineDisposable()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
    public class ClientStateMachine: StateMachine
    {
        protected override void HandleError(StateMachineErrorData errorData)
        {
            Debug.LogException(errorData.Exception);
        }
    }
}