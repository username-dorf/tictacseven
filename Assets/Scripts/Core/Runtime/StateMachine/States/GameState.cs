using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.StateMachine
{
    public interface IGameBootstrapAsync: IDisposable
    {
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }
    public class GameState: SceneState
    {
        private IGameBootstrapAsync _gameBootstrap;

        public override async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            await LoadSceneAsync("Game", cancellationToken);
            
            var sceneContext = GameObject.FindFirstObjectByType<SceneContext>();
            _gameBootstrap = sceneContext.Container.Resolve<IGameBootstrapAsync>();
            await _gameBootstrap.InitializeAsync(cancellationToken);
        }

        public override async UniTask ExitAsync(CancellationToken cancellationToken)
        {
            _gameBootstrap?.Dispose();
        }
    }
}