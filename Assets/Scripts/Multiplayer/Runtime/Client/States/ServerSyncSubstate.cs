using System;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.Field;
using Game.States;
using Game.User;
using Multiplayer.Contracts;
using UniRx;
using UniState;
using UnityEngine;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Client.States
{
    public class ServerSyncSubstate : GameSubstate<ClientFieldSync>
    {
        private LazyInject<FieldModel> _fieldModel;
        private LazyInject<IStateProviderDebug> _stateProviderDebug;
        private LazyInject<UserEntitiesModel> _opponentEntitiesModel;
        private FieldViewProvider _fieldViewProvider;
        
        private UserEntitiesController _entitiesController;
        
        private ReactiveCommand<ClientTurn> _onTurnReceived;
        private ReactiveCommand<RoundResult> _onRoundResultReceived;

        public ServerSyncSubstate(
            FieldViewProvider fieldViewProvider,
            LazyInject<FieldModel> fieldModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<UserEntitiesModel> opponentEntitiesModel,
            [InjectOptional] LazyInject<IStateProviderDebug> stateProviderDebug)
        {
            _stateProviderDebug = stateProviderDebug;
            _fieldViewProvider = fieldViewProvider;
            _opponentEntitiesModel = opponentEntitiesModel;
            _fieldModel = fieldModel;

            _onTurnReceived = new ReactiveCommand<ClientTurn>();
            _onRoundResultReceived = new ReactiveCommand<RoundResult>();
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _stateProviderDebug?.Value?.ChangeState(this);
            AddDisposables();

            _entitiesController ??= new UserEntitiesController(_fieldModel.Value, _opponentEntitiesModel.Value, _fieldViewProvider);
            InstanceFinder.ClientManager.RegisterBroadcast<ClientTurn>(OnTurnReceived);
            InstanceFinder.ClientManager.RegisterBroadcast<RoundResult>(OnRoundResultReceived);

            await OnSyncRequest(Payload, token);
            
            using var raceCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var roundResultTask  = WaitRoundResultReceivedAsync(raceCts.Token);
            var turnTask = WaitTurnReceivedAsync(raceCts.Token);

            var (i, roundResult, turnResult) = await UniTask.WhenAny(roundResultTask, turnTask);

            raceCts.Cancel();
            
            return i switch
            {
                0 => roundResult,                
                1 => turnResult,                
                _ => Transition.GoToExit()  
            };
            
        }
        public override UniTask Exit(CancellationToken ct)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurn>(OnTurnReceived);
            InstanceFinder.ClientManager.UnregisterBroadcast<RoundResult>(OnRoundResultReceived);
            return base.Exit(ct);
        }

        private void OnRoundResultReceived(RoundResult arg1, Channel arg2)
        {
            _onRoundResultReceived?.Execute(arg1);
        }
        private async UniTask<StateTransitionInfo> WaitRoundResultReceivedAsync(
            CancellationToken token)
        {
            var payload = await _onRoundResultReceived
                .First()
                .ToUniTask(cancellationToken: token);
            return Transition.GoTo<RoundResultSubstate,RoundResult>(payload);
        }

        private void OnTurnReceived(ClientTurn arg1, Channel arg2)
        {
            _onTurnReceived?.Execute(arg1);
        }
        private async UniTask<StateTransitionInfo> WaitTurnReceivedAsync(
            CancellationToken token)
        {
            var payload = await _onTurnReceived
                .First()
                .ToUniTask(cancellationToken: token);
            return Transition.GoTo<TurnSubstate,ClientTurn>(payload);
        }
        
        private async UniTask OnSyncRequest(ClientFieldSync request, CancellationToken token)
        {
            var needSync = _fieldModel.Value.FindDifference(request.FieldModelState, out var diff);
            if (!needSync)
            {
                var message = "No need to sync with server";
                Debug.Log(message);
                BroadcastSyncDone(message);
                return;
            }

            Debug.Log("Syncing with server. Applying opponent move");
            
            await DoSyncAsync(diff.snapshot.Merit, diff.coors, token);
        }

        private async UniTask DoSyncAsync(int merit, Vector2Int coors, CancellationToken token)
        {
            try
            {
                await _entitiesController.DoMoveAsync(merit, coors, token);
                BroadcastSyncDone();
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Sync opponent move was canceled");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void BroadcastSyncDone(string reason = null)
        {
            var response = new ClientFieldSyncResponse
            {
                Accepted = reason == null,
                Reason = reason,
            };
            InstanceFinder.ClientManager.Broadcast(response);
        }

        private void AddDisposables()
        {
            _onTurnReceived?.AddTo(Disposables);
            _onRoundResultReceived?.AddTo(Disposables);
            
            Disposables.Add(() => InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurn>(OnTurnReceived));
            Disposables.Add(() => InstanceFinder.ClientManager.UnregisterBroadcast<RoundResult>(OnRoundResultReceived));
        }
    }
}
