using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Game.Entities;
using Game.Field;
using Game.User;
using Multiplayer.Connection;
using Multiplayer.Contracts;
using UniRx;
using UniState;
using UnityEngine;
using Zenject;
using Channel = FishNet.Transporting.Channel;


namespace Multiplayer.Server.States
{
    public class FieldSyncSubstate : ServerSubstate<ClientTurnResponse>
    {
        private LazyInject<FieldModel> _fieldModel;
        private LazyInject<IServerClientsProvider> _clientsProvider;
        private LazyInject<IServerActiveClientProvider> _activeClientProvider;
        private LazyInject<IServerRoundCounter> _roundCounter;

        private LazyInject<IStateProviderDebug> _stateProviderDebug;

        private ReactiveProperty<int> _syncedClientsCount;
        private HashSet<int> _whoReplied;


        public FieldSyncSubstate(
            LazyInject<FieldModel> fieldModel,
            LazyInject<IServerClientsProvider> clientsProvider,
            LazyInject<IServerActiveClientProvider> activeClientProvider,
            [InjectOptional] LazyInject<IStateProviderDebug> stateProviderDebug,
            LazyInject<IServerRoundCounter> roundCounter)
        {
            _roundCounter = roundCounter;
            _stateProviderDebug = stateProviderDebug;
            _whoReplied = new HashSet<int>();
            _syncedClientsCount = new ReactiveProperty<int>();
            _activeClientProvider = activeClientProvider;
            _clientsProvider = clientsProvider;
            _fieldModel = fieldModel;
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _stateProviderDebug?.Value?.ChangeState(this);
            AddDisposables();

            InstanceFinder.ServerManager.RegisterBroadcast<ClientFieldSyncResponse>(OnClientSynced);

            UpdateEntitiesModel(Payload.ClientId, Payload.Merit);
            var clientOwnerValue = _clientsProvider.Value.GetClientOwnerValue(Payload.ClientId);
            OnClientTurn(clientOwnerValue, Payload.Merit, Payload.Coordinates);

            InstanceFinder.ServerManager.Broadcast(new ClientFieldSync()
            {
                FieldModelState = _fieldModel.Value.GetDataSnapshot(),
            });

            try
            {
                await _syncedClientsCount
                    .Where(v => v >= ConnectionConfig.MAX_CLIENTS)
                    .First()
                    .ToUniTask(cancellationToken: token);

                return OnAllClientsSynced();
            }
            catch
            {
                // ignored
            }

            return Transition.GoToExit();
        }

        public override UniTask Exit(CancellationToken token)
        {
            _syncedClientsCount.Value = 0;
            _whoReplied.Clear();

            InstanceFinder.ServerManager.UnregisterBroadcast<ClientFieldSyncResponse>(OnClientSynced);
            return base.Exit(token);
        }

        private void OnClientTurn(int owner, int merit, Vector2Int coors)
        {
            var model = new EntityModel(merit, owner, Vector3.zero);
            _fieldModel.Value.UpdateEntity(coors, Vector3.zero, model);
        }

        private void OnClientSynced(NetworkConnection connection, ClientFieldSyncResponse response, Channel arg3)
        {
            if (_whoReplied.Add(connection.ClientId))
            {
                _syncedClientsCount.Value = _whoReplied.Count;
            }
        }

        private StateTransitionInfo OnAllClientsSynced()
        {
            _activeClientProvider.Value.ChangeActiveClientId();
            var hasWinners = HasWinner(out var winners);
            if (hasWinners)
            {
                var payload = new RoundResultSubstate.PayloadModel
                {
                    PassedRounds = _roundCounter.Value.PassedRounds.Value,
                    WinnerIds = winners,
                };
                return Transition.GoTo<RoundResultSubstate, RoundResultSubstate.PayloadModel>(payload);
            }

            return Transition.GoTo<ClientTurnSubstate>();
        }

        private void UpdateEntitiesModel(string clientId, int merit)
        {
            var model = _clientsProvider.Value.ClientEntitiesModels[clientId];
            var placed = model.Entities.First(x => x.Data.Merit.Value == merit);
            model.Entities.Remove(placed);
        }

        private string GetClientId(int owner, ReactiveDictionary<string, UserEntitiesModel> models)
        {
            foreach (var (key, value) in models)
            {
                if (value.Owner == owner)
                    return key;
            }

            return string.Empty;
        }

        private bool HasWinner(out List<string> winners)
        {
            winners = new List<string>();
            var winnerOwner = _fieldModel.Value.GetWinner();
            if (winnerOwner.HasValue)
            {
                _roundCounter.Value.IncrementPassedRound();

                var winnerId = GetClientId(winnerOwner.Value, _clientsProvider.Value.ClientEntitiesModels);
                winners.Add(winnerId);
                return true;
            }

            var activeUserEntitiesModel =
                _clientsProvider.Value.ClientEntitiesModels[_activeClientProvider.Value.ActiveClientId.Value];
            var isDraw = _fieldModel.Value.IsDraw(activeUserEntitiesModel);
            if (isDraw)
            {
                _roundCounter.Value.IncrementPassedRound();

                winners.AddRange(_clientsProvider.Value.ClientEntitiesModels.Keys);
                return true;
            }

            return false;
        }

        private void AddDisposables()
        {
            Disposables.Add(() =>
                InstanceFinder.ServerManager.UnregisterBroadcast<ClientFieldSyncResponse>(OnClientSynced));
            _syncedClientsCount.AddTo(Disposables);
        }
    }
}