using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Game.Entities;
using Game.Field;
using Game.States;
using Game.User;
using Multiplayer.Connection;
using Multiplayer.Contracts;
using UniRx;
using UnityEngine;
using Zenject;
using Channel = FishNet.Transporting.Channel;


namespace Multiplayer.Server.States
{
    public class FieldSyncSubstate : ServerSubstate<ClientTurnResponse>
    {
        private FieldModel _fieldModel;
        private IServerClientsProvider _clientsProvider;
        private IServerActiveClientProvider _activeClientProvider;

        private ReactiveProperty<int> _syncedClientsCount;
        private readonly HashSet<int> _whoReplied;
        private int _passedRounds;
       

        public FieldSyncSubstate(
            FieldModel fieldModel,
            IServerClientsProvider clientsProvider,
            IServerActiveClientProvider activeClientProvider,
            IServerSubstateResolver substateResolverFactory) : base(substateResolverFactory)
        {
            _activeClientProvider = activeClientProvider;
            _whoReplied = new HashSet<int>();
            _syncedClientsCount = new ReactiveProperty<int>();
            _clientsProvider = clientsProvider;
            _fieldModel = fieldModel;
        }

        protected override async UniTask EnterAsync(ClientTurnResponse payload, CancellationToken ct)
        {
            InstanceFinder.ServerManager.RegisterBroadcast<ClientFieldSyncResponse>(OnClientSynced);

            UpdateEntitiesModel(payload.ClientId,payload.Merit);
            var clientOwnerValue = _clientsProvider.GetClientOwnerValue(payload.ClientId);
            OnClientTurn(clientOwnerValue, payload.Merit,payload.Coordinates);
            InstanceFinder.ServerManager.Broadcast(new ClientFieldSync()
            {
                FieldModelState = _fieldModel.GetDataSnapshot(),
            });
            
            try
            {
                await _syncedClientsCount
                    .Where(v => v >= ConnectionConfig.MAX_CLIENTS)
                    .First()                            
                    .ToUniTask(cancellationToken: ct); 
                
                await OnAllClientsSynced(ct);
            }
            catch
            {
                // ignored
            }

        }
        
        public override UniTask ExitAsync(CancellationToken ct)
        {
            _syncedClientsCount.Value = 0;
            _whoReplied.Clear();
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientFieldSyncResponse>(OnClientSynced);
            return UniTask.CompletedTask;
        }

        private void OnClientTurn(int owner, int merit, Vector2Int coors)
        {
            var model = new EntityModel(merit, owner, Vector3.zero);
            _fieldModel.UpdateEntity(coors, Vector3.zero, model);
        }
        
        private void OnClientSynced(NetworkConnection connection, ClientFieldSyncResponse response, Channel arg3)
        {
            if (_whoReplied.Add(connection.ClientId))
            {
                _syncedClientsCount.Value = _whoReplied.Count;
            }
        }

        private async UniTask OnAllClientsSynced(CancellationToken ct)
        {
            Debug.Log("All clients synced");
            _activeClientProvider.ChangeActiveClientId();
            var hasWinners = HasWinner(out var winners);
            if (hasWinners)
            {
                var payload = new RoundResultSubstate.Payload
                {
                    PassedRounds = _passedRounds,
                    WinnerIds = winners,
                };
                await SubstateMachine.ChangeStateAsync<RoundResultSubstate,RoundResultSubstate.Payload>(payload, 
                    CancellationToken.None);
                return;

            }
            await SubstateMachine.ChangeStateAsync<ClientTurnSubstate>(CancellationToken.None);
        }

        private void UpdateEntitiesModel(string clientId, int merit)
        {
            var model = _clientsProvider.ClientEntitiesModels[clientId];
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
            var winnerOwner = _fieldModel.GetWinner();
            if (winnerOwner.HasValue)
            {
                _passedRounds++;

                var winnerId = GetClientId(winnerOwner.Value, _clientsProvider.ClientEntitiesModels);
                winners.Add(winnerId);
                return true;
            }
            
            var activeUserEntitiesModel = _clientsProvider.ClientEntitiesModels[_activeClientProvider.ActiveClientId.Value];
            var isDraw = _fieldModel.IsDraw(activeUserEntitiesModel);
            if (isDraw)
            {
                _passedRounds++;
                winners.AddRange(_clientsProvider.ClientEntitiesModels.Keys);
                return true;
            }

            return false;
        }

        public override void Dispose()
        {
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientFieldSyncResponse>(OnClientSynced);
        }
    }
}