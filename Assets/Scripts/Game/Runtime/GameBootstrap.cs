using System;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.States;
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
            await substateMachine.ChangeStateAsync<InitialSubstate>(ct);
        }

        public void Dispose()
        {
            _cts.Cancel();
        }

        public void Initialize()
        {
            UniTask.Void(async () =>
            {
                try
                {
                    Debug.Log("[GameBootstrap] Initialize enter");
                    await InitializeAsync(_cts.Token);
                    Debug.Log("[GameBootstrap] Initialize done");
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