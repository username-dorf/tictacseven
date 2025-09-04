using System;
using System.Threading;
using Core.StateMachine;
using Core.User;
using FishNet;
using Cysharp.Threading.Tasks;
using Game.States;
using Multiplayer.Client.States;
using Multiplayer.Contracts;
using UnityEngine;
using Zenject;
using Channel = FishNet.Transporting.Channel;
using InitialSubstate = Multiplayer.Client.States.InitialSubstate;


namespace Multiplayer.Client
{
    public class ClientSessionBootstrap : IDisposable
    {
        private IStateMachine _stateMachine;
        private SceneContextRegistry _sceneContextRegistry;
        private IStateMachine _substateMachine;

        public ClientSessionBootstrap(IStateMachine stateMachine, SceneContextRegistry sceneContextRegistry)
        {
            _sceneContextRegistry = sceneContextRegistry;
            _stateMachine = stateMachine;
        }
        public void Launch()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<ClientInitialization>(OnClientInitializationRequest);
        }

        private void OnClientInitializationRequest(ClientInitialization request,
            Channel channel)
        {
            _stateMachine.ChangeStateAsync<MultiplayerGameState>(CancellationToken.None)
                .ContinueWith(()=>SubstateBootstrap(request, CancellationToken.None))
                .Forget();
        }

        private async UniTask SubstateBootstrap(ClientInitialization request, CancellationToken ct)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var container = _sceneContextRegistry.GetContainerForScene(scene);
            
            await UniTask.WaitUntil(
                () => container.HasBinding<IGameSubstateResolver>(),
                cancellationToken: ct
            );
            
            var resolver = container.Resolve<IGameSubstateResolver>();
            _substateMachine = resolver.Resolve<IStateMachine>();

            var payload = new InitialSubstate.Payload
            {
                Owner = request.Owner,
                OpponentOwner = request.OpponentOwner,
                OpponentPreferences = request.Opponent
            };
            await _substateMachine.ChangeStateAsync<InitialSubstate,InitialSubstate.Payload>(payload, ct);
        }

        public void Dispose()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientInitialization>(OnClientInitializationRequest);
        }
    }
}