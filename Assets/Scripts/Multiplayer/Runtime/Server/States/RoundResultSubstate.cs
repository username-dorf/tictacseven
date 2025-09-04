using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Game.Field;
using Multiplayer.Connection;
using Multiplayer.Contracts;
using UniRx;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Server.States
{
    public class RoundResultSubstate : ServerSubstate<RoundResultSubstate.Payload>
    {
        private ReactiveProperty<int> _repliedClientsCount;
        private readonly HashSet<int> _whoReplied;
        
        private FieldModel _fieldModel;
        private IServerClientsProvider _clientsProvider;

        public struct Payload
        {
            public int PassedRounds;
            public List<string> WinnerIds;
        }
        
        public RoundResultSubstate(
            FieldModel fieldModel,
            IServerClientsProvider clientsProvider,
            IServerSubstateResolver substateResolverFactory) 
            : base(substateResolverFactory)
        {
            _clientsProvider = clientsProvider;
            _fieldModel = fieldModel;
            _whoReplied = new HashSet<int>();
            _repliedClientsCount = new ReactiveProperty<int>();
        }

        protected override async UniTask EnterAsync(Payload payload, CancellationToken ct)
        {
            InstanceFinder.ServerManager.RegisterBroadcast<RoundResultResponse>(OnRoundResultResponse);

            InstanceFinder.ServerManager.Broadcast(new RoundResult()
            {
                WinnerIds = payload.WinnerIds
            });
            
            try
            {
                await _repliedClientsCount
                    .Where(v => v >= ConnectionConfig.MAX_CLIENTS)
                    .First()                            
                    .ToUniTask(cancellationToken: ct); 
                
                await OnAllRoundResultResponse(payload.PassedRounds, ct);
            }
            catch
            {
                // ignored
            }
        }

        private void OnRoundResultResponse(NetworkConnection connection, RoundResultResponse arg2, Channel arg3)
        {
            if (_whoReplied.Add(connection.ClientId))
            {
                _repliedClientsCount.Value = _whoReplied.Count;
            }
        }

        private async UniTask OnAllRoundResultResponse(int passedRounds, CancellationToken ct)
        {
            if (passedRounds >= FieldConfig.ROUNDS_AMOUNT)
            {
                InstanceFinder.ServerManager.Broadcast(new TerminateSession());
                return;
            }
            
            _fieldModel?.Drop();
            _clientsProvider?.Drop();
            await SubstateMachine.ChangeStateAsync<ClientTurnSubstate>(ct);
        }
        
        public override UniTask ExitAsync(CancellationToken ct)
        {
            _whoReplied.Clear();
            InstanceFinder.ServerManager.UnregisterBroadcast<RoundResultResponse>(OnRoundResultResponse);
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            InstanceFinder.ServerManager.UnregisterBroadcast<RoundResultResponse>(OnRoundResultResponse);
            _whoReplied.Clear();
            _repliedClientsCount?.Dispose();
        }
    }
}