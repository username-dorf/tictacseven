using System.Collections.Generic;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Multiplayer.Client;
using Multiplayer.Connection;
using Multiplayer.Contracts;
using UniRx;
using UniState;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Server.States
{
    public class InitializeClientsSubstate : ServerSubstate<InitializeClientsSubstate.PayloadModel>
    {
        private ReactiveProperty<int> _initializedClientsCount;
        private HashSet<int> _whoReplied;

        private LazyInject<IServerClientsRegister> _clientsRegister;
        private LazyInject<IStateProviderDebug> _stateProviderDebug;

        public struct PayloadModel
        {
            public ClientConnection Current { get; }
            public ClientConnection Opponent { get; }

            public PayloadModel(ClientConnection current, ClientConnection opponent)
            {
                Current = current;
                Opponent = opponent;
            }
        }

        public InitializeClientsSubstate(LazyInject<IServerClientsRegister> clientsRegister,
            [InjectOptional] LazyInject<IStateProviderDebug> stateProviderDebug)
        {
            _stateProviderDebug = stateProviderDebug;
            _clientsRegister = clientsRegister;
            _whoReplied = new HashSet<int>();
            _initializedClientsCount = new ReactiveProperty<int>();
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken ct)
        {
            _stateProviderDebug?.Value?.ChangeState(this);
            AddDisposables();

            var ownerClient = Payload.Current;
            var ownerValue = _clientsRegister.Value.RegisterClient(ownerClient);
            var opponentClient = Payload.Opponent;
            var opponentOwnerValue = _clientsRegister.Value.RegisterClient(opponentClient);

            InstanceFinder.ServerManager.Broadcast(ownerClient.Connection, new ClientInitialization()
            {
                Owner = ownerValue,
                OpponentOwner = opponentOwnerValue,
                Opponent = opponentClient.Preferences
            });

            InstanceFinder.ServerManager.Broadcast(opponentClient.Connection, new ClientInitialization()
            {
                Owner = opponentOwnerValue,
                OpponentOwner = ownerValue,
                Opponent = ownerClient.Preferences
            });

            InstanceFinder.ServerManager.RegisterBroadcast<ClientInitializationResponse>(OnClientInitialized);

            try
            {
                await _initializedClientsCount
                    .Where(v => v >= ConnectionConfig.MAX_CLIENTS)
                    .First()
                    .ToUniTask(cancellationToken: ct);

                return Transition.GoTo<ClientTurnSubstate>();
            }
            catch
            {
                return Transition.GoToExit();
            }
        }

        public override UniTask Exit(CancellationToken token)
        {
            _whoReplied.Clear();
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientInitializationResponse>(OnClientInitialized);
            return base.Exit(token);
        }

        private void OnClientInitialized(NetworkConnection connection, ClientInitializationResponse response,
            Channel channel)
        {
            if (_whoReplied.Add(connection.ClientId))
            {
                _initializedClientsCount.Value = _whoReplied.Count;
            }
        }

        private void AddDisposables()
        {
            Disposables.Add(() =>
                InstanceFinder.ServerManager.UnregisterBroadcast<ClientInitializationResponse>(OnClientInitialized));
            _initializedClientsCount.AddTo(Disposables);
        }
    }
}