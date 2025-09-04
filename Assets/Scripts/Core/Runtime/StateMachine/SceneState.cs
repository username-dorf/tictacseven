using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Core.StateMachine
{
    public abstract class SceneState : IState
    {
        
        public abstract UniTask EnterAsync(CancellationToken ct);
        public abstract UniTask ExitAsync(CancellationToken ct);
        
        protected async UniTask LoadSceneAsync(string sceneName, CancellationToken cancellationToken)
        {
            var handle = Addressables.LoadSceneAsync(sceneName);

            try
            {
                await handle.ToUniTask(cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                throw;
            }
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}