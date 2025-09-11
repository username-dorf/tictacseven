using System;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.States;
using UniState;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameBootstrap : IInitializable, IGameBootstrapAsync
    {
        
        private IGameSubstateResolver _gameSubstateResolver;
        private readonly CancellationTokenSource _cts = new();


        public GameBootstrap(
            IGameSubstateResolver gameSubstateResolver)
        {
            _gameSubstateResolver = gameSubstateResolver;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            var substateMachine = _gameSubstateResolver.Resolve<IStateMachine>();
            await substateMachine.Execute<InitialSubstate>(ct);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void Initialize()
        {
            UniTask.Void(async () =>
            {
                try
                {
                    await InitializeAsync(_cts.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
        }
    }
}