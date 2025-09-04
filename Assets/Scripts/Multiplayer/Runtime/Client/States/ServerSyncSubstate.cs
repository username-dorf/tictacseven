using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.Field;
using Game.States;
using Game.User;
using Multiplayer.Contracts;
using UnityEngine;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Client.States
{
    public class ServerSyncSubstate : GameSubstate
    {
        private FieldModel _fieldModel;
        private UserEntitiesController _entitiesController;
        private CancellationTokenSource _syncCts;

        public ServerSyncSubstate(
            FieldModel fieldModel,
            FieldViewProvider fieldViewProvider,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] UserEntitiesModel opponentEntitiesModel,
            IGameSubstateResolver substateResolverFactory) : base(substateResolverFactory)
        {
            _fieldModel = fieldModel;
            _entitiesController = new UserEntitiesController(fieldModel, opponentEntitiesModel, fieldViewProvider);
        }

        public override UniTask EnterAsync(CancellationToken ct)
        {
            Debug.Log("Entered sync state");
            InstanceFinder.ClientManager.RegisterBroadcast<ClientFieldSync>(OnSyncRequest);
            return UniTask.CompletedTask;
        }

        private void OnSyncRequest(ClientFieldSync request, Channel arg2)
        {
            var needSync = _fieldModel.FindDifference(request.FieldModelState,out var diff);
            if (!needSync)
            {
                var message = "No need to sync with server";
                Debug.Log(message);
                BroadcastSyncDone(message);
                return;
            }

            Debug.Log("Syncing with server. Applying opponent move");
            _syncCts?.Cancel();
            _syncCts?.Dispose();
            
            _syncCts = new CancellationTokenSource();
            UniTask.Void(async () =>
            {
                try
                {
                    await _entitiesController.DoMoveAsync(diff.snapshot.Merit, diff.coors, _syncCts.Token);
                    BroadcastSyncDone();

                }
                catch (OperationCanceledException)
                {
                    Debug.LogError("Sync opponent move was canceled");
                }
            });
        }

        public override UniTask ExitAsync(CancellationToken ct)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientFieldSync>(OnSyncRequest);
            return UniTask.CompletedTask;
        }

        private void BroadcastSyncDone(string reason = null)
        {
            var response = new ClientFieldSyncResponse()
            {
                Accepted = true,
                Reason = reason,
            };
            InstanceFinder.ClientManager.Broadcast(response);
        }

        public override void Dispose()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientFieldSync>(OnSyncRequest);
            _syncCts?.Dispose();
        }
    }
}