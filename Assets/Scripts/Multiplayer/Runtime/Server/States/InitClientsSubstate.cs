using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Multiplayer.Client;
using Multiplayer.Connection;
using Multiplayer.Contracts;
using UniRx;
using UnityEngine;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Server.States
{
    public class InitClientsSubstate : ServerSubstate<InitClientsSubstate.Payload>
    {
        private ReactiveProperty<int> _initializedClientsCount;
        private readonly HashSet<int> _whoReplied;
        private IServerClientsRegister _clientsRegister;

        public class Payload
        {
            public ClientConnection Current { get; }
            public ClientConnection Opponent { get; }

            public Payload(ClientConnection current, ClientConnection opponent)
            {
                Current = current;
                Opponent = opponent;
            }
        }
        
        public InitClientsSubstate(
            IServerClientsRegister clientsRegister,
            IServerSubstateResolver substateResolverFactory) 
            : base(substateResolverFactory)
        {
            _clientsRegister = clientsRegister;
            _whoReplied = new HashSet<int>();
            _initializedClientsCount = new ReactiveProperty<int>();
        }

        protected override async UniTask EnterAsync(Payload payload, CancellationToken ct)
        {
            var ownerClient = payload.Current;
            var ownerValue = _clientsRegister.RegisterClient(ownerClient);
            var opponentClient = payload.Opponent;
            var opponentOwnerValue =_clientsRegister.RegisterClient(opponentClient);
            
            InstanceFinder.ServerManager.Broadcast(ownerClient.Connection, new ClientInitialization()
            {
                Owner = ownerValue,
                OpponentOwner = opponentOwnerValue,
                Opponent =opponentClient.Preferences
            });
            
            InstanceFinder.ServerManager.Broadcast(opponentClient.Connection, new ClientInitialization()
            {
                Owner = opponentOwnerValue,
                OpponentOwner = ownerValue,
                Opponent =ownerClient.Preferences
            });
            
            InstanceFinder.ServerManager.RegisterBroadcast<ClientInitializationResponse>(OnClientInitialized);

            try
            {
                await _initializedClientsCount
                    .Where(v => v >= ConnectionConfig.MAX_CLIENTS)
                    .First()                            
                    .ToUniTask(cancellationToken: ct); 
                
                await OnAllClientsInitialized(ct);
            }
            catch
            {
                // ignored
            }
        }

        public override UniTask ExitAsync(CancellationToken ct)
        {
            _whoReplied.Clear();
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientInitializationResponse>(OnClientInitialized);
            return UniTask.CompletedTask;
        }

        private void OnClientInitialized(NetworkConnection connection, ClientInitializationResponse response, Channel channel)
        {
            if (_whoReplied.Add(connection.ClientId))
            {
                _initializedClientsCount.Value = _whoReplied.Count;
            }
        }

        private async UniTask OnAllClientsInitialized(CancellationToken ct)
        {
            Debug.Log("All clients initialized");
            await SubstateMachine.ChangeStateAsync<ClientTurnSubstate>(CancellationToken.None);
        }

        public override void Dispose()
        {
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientInitializationResponse>(OnClientInitialized);
            _whoReplied.Clear();
            _initializedClientsCount?.Dispose();
        }
    }
}