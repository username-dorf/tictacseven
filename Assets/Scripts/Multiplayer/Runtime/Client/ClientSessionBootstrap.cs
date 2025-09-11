using System;
using System.Threading;
using Core.StateMachine;
using FishNet;
using Cysharp.Threading.Tasks;
using Game.States;
using Multiplayer.Contracts;
using UniState;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Channel = FishNet.Transporting.Channel;
using InitialSubstate = Multiplayer.Client.States.InitialSubstate;


namespace Multiplayer.Client
{
    public class ClientSessionBootstrap : IDisposable
    {
        private SceneContextRegistry _sceneContextRegistry;
        private IStateMachine _substateMachine;
        private ManualTransitionTrigger<MultiplayerGameState> _multTransitionTrigger;

        public ClientSessionBootstrap(ManualTransitionTrigger<MultiplayerGameState> multTransitionTrigger,
            SceneContextRegistry sceneContextRegistry)
        {
            _multTransitionTrigger = multTransitionTrigger;
            _sceneContextRegistry = sceneContextRegistry;
        }

        public void Launch()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<ClientInitialization>(OnClientInitializationRequest);
        }

        private void OnClientInitializationRequest(ClientInitialization request,
            Channel channel)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    await UniTask.SwitchToMainThread();
                    _multTransitionTrigger.Continue();
                    await _multTransitionTrigger.WhenArrivedAsync(CancellationToken.None);
                    await SubstateBootstrap(request, CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
        }

        private async UniTask SubstateBootstrap(ClientInitialization request, CancellationToken ct)
        {
            await UniTask.WaitUntil(() =>
            {
                var active = SceneManager.GetActiveScene();
                var c = _sceneContextRegistry.GetContainerForScene(active);
                return c != null && c.HasBinding<IGameSubstateResolver>();
            }, cancellationToken: ct);

            var scene = SceneManager.GetActiveScene();
            var container = _sceneContextRegistry.GetContainerForScene(scene);

            var resolver = container.Resolve<IGameSubstateResolver>();
            _substateMachine = resolver.Resolve<IStateMachine>();
            var stateMachineDisposable = resolver.Resolve<ClientStateMachineDisposable>();

            var payload = new InitialSubstate.PayloadModel
            {
                Owner = request.Owner,
                OpponentOwner = request.OpponentOwner,
                OpponentPreferences = request.Opponent
            };
            await _substateMachine.Execute<InitialSubstate, InitialSubstate.PayloadModel>(payload,
                stateMachineDisposable.Token);
        }

        public void Dispose()
        {
            try
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<ClientInitialization>(OnClientInitializationRequest);
            }
            catch (Exception)
            {
            }
        }
    }
}