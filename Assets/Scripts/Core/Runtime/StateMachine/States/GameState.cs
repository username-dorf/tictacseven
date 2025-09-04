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

        public override async UniTask EnterAsync(CancellationToken ct)
        {
            await LoadSceneAsync("Game", ct);
        }

        public override async UniTask ExitAsync(CancellationToken ct)
        {
            _gameBootstrap?.Dispose();
        }
    }
}