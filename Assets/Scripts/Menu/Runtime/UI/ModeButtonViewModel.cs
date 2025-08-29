using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Menu.UI
{
    public class ModeButtonViewModel : IDisposable
    {
        private readonly Func<CancellationToken, UniTask> _actionFactory;

        public ModeButtonViewModel(Func<CancellationToken, UniTask> actionFactory)
        {
            _actionFactory = actionFactory;
        }

        public async UniTask ExecuteAsync(CancellationToken ct = default)
        {
            try
            {
                await _actionFactory(ct);
            }
            catch (OperationCanceledException)
            {
            
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }

        public void Dispose()
        {
        
        }
    }
}