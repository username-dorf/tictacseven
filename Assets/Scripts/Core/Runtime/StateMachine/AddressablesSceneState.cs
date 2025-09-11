using System.Threading;
using Cysharp.Threading.Tasks;
using UniState;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Core.StateMachine
{
    public abstract class AddressablesSceneStateBase : StateBase
    {
        private AsyncOperationHandle<SceneInstance> _loadHandle;

        protected SceneInstance SceneInstance { get; private set; }
        protected bool IsLoaded { get; private set; }

        protected abstract string SceneAddress { get; }

        protected virtual LoadSceneMode LoadMode => LoadSceneMode.Single;

        protected virtual bool ActivateOnLoad => true;

        protected virtual bool SetActiveOnReady => true;

        protected virtual int Priority => 100;

        protected virtual bool UnloadOnExit => true;

        protected virtual UniTask OnSceneReady(CancellationToken token) => UniTask.CompletedTask;

        protected abstract UniTask<StateTransitionInfo> ExecuteAfterSceneReady(CancellationToken token);

        public float Progress => _loadHandle.IsValid() ? _loadHandle.PercentComplete : 0f;

        public override async UniTask Initialize(CancellationToken token)
        {
            _loadHandle = Addressables.LoadSceneAsync(SceneAddress, LoadMode, ActivateOnLoad, Priority);
            SceneInstance = await _loadHandle.ToUniTask(cancellationToken: token);

            if (!ActivateOnLoad)
            {
                var activateHandle = SceneInstance.ActivateAsync();
                await activateHandle.ToUniTask(cancellationToken: token);
            }

            if (SetActiveOnReady)
                SceneManager.SetActiveScene(SceneInstance.Scene);

            IsLoaded = true;

            await OnSceneReady(token);
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            return await ExecuteAfterSceneReady(token);
        }

        public override async UniTask Exit(CancellationToken token)
        {
            if (!IsLoaded) return;

            if (UnloadOnExit && _loadHandle.IsValid())
                await Addressables.UnloadSceneAsync(_loadHandle, true).ToUniTask(cancellationToken: token);

            IsLoaded = false;
            SceneInstance = default;
            _loadHandle = default;
        }
    }
}