using System;
using System.Threading;
using Core.StateMachine;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.Entities;
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
    public class TurnSubstate: GameSubstate<ClientTurn>
    {
        private LazyInject<FieldModel> _fieldModel;
        private LazyInject<UserRoundModel> _userRoundModel;
        private LazyInject<UserEntitiesModel> _userEntitiesModel;
        private LazyInject<UserRoundModel> _opponentRoundModel;
        private LazyInject<IStateProviderDebug> _stateProviderDebug;

        private IUserPreferencesProvider _userPreferencesProvider;

        private ReactiveCommand<ClientFieldSync> _onFieldSyncReceived;
        private CancellationTokenSource _waitCts;

        public TurnSubstate(
            LazyInject<FieldModel> fieldModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<UserRoundModel> opponentRoundModel,
            [Inject(Id = UserModelConfig.ID)] LazyInject<UserRoundModel> userRoundModel,
            [Inject(Id = UserModelConfig.ID)] LazyInject<UserEntitiesModel> userEntitiesModel,
            [InjectOptional] LazyInject<IStateProviderDebug> stateProviderDebug,
            IUserPreferencesProvider userPreferencesProvider)
        {
            _stateProviderDebug = stateProviderDebug;
            _opponentRoundModel = opponentRoundModel;
            _userEntitiesModel = userEntitiesModel;
            _userPreferencesProvider = userPreferencesProvider;
            _userRoundModel = userRoundModel;
            _fieldModel = fieldModel;

            _onFieldSyncReceived = new ReactiveCommand<ClientFieldSync>();
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken ct)
        {
            _stateProviderDebug?.Value?.ChangeState(this);
            AddDisposables();

            var isCurrentClientActive = _userPreferencesProvider.Current.User.Id == Payload.ActiveClientId;
            _userRoundModel.Value.SetAwaitingTurn(isCurrentClientActive);
            _opponentRoundModel.Value.SetAwaitingTurn(!isCurrentClientActive);

            _userEntitiesModel.Value.SetInteractionAll(isCurrentClientActive);
            if (!isCurrentClientActive)
            {
                return Transition.GoTo<WaitTurnSubstate>();
            }

            InstanceFinder.ClientManager.RegisterBroadcast<ClientTurnTimeout>(OnTurnTimeout);
            InstanceFinder.ClientManager.RegisterBroadcast<ClientFieldSync>(OnTurnSync);

            _waitCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _waitCts.AddTo(Disposables);

            try
            {
                var move = await _fieldModel.Value.OnEntityChanged
                    .First()
                    .ToUniTask(cancellationToken: _waitCts.Token);

                if (_waitCts.IsCancellationRequested)
                    return OnTimeoutGoToWait();

                OnTurnDone(move);

                var sync = await _onFieldSyncReceived
                    .First()
                    .ToUniTask(cancellationToken: _waitCts.Token);

                return Transition.GoTo<ServerSyncSubstate, ClientFieldSync>(sync);
            }
            catch (OperationCanceledException)
            {
                return OnTimeoutGoToWait();
            }

        }

        public override UniTask Exit(CancellationToken ct)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientFieldSync>(OnTurnSync);
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurnTimeout>(OnTurnTimeout);
            
            _waitCts?.Cancel();
            _waitCts?.Dispose();
            _waitCts = null;
            
            return base.Exit(ct);
        }

        private void OnTurnDone((Vector2Int coors, EntityModel model) value)
        {
            var response = new ClientTurnResponse()
            {
                ClientId = _userPreferencesProvider.Current.User.Id,
                Merit = value.model.Data.Merit.Value,
                Coordinates = value.coors,
            };
            InstanceFinder.ClientManager.Broadcast(response);
            _userEntitiesModel.Value.SetInteractionAll(false);
        }

        private void OnTurnTimeout(ClientTurnTimeout request, Channel channel)
        {
            _userEntitiesModel.Value.SetInteractionAll(false);
            _waitCts?.Cancel();
        }

        private void OnTurnSync(ClientFieldSync clientFieldSync, Channel channel)
        {
            _onFieldSyncReceived?.Execute(clientFieldSync);
        }

        private StateTransitionInfo OnTimeoutGoToWait()
        {
            _userRoundModel.Value.SetAwaitingTurn(false);
            _opponentRoundModel.Value.SetAwaitingTurn(true);
            _userEntitiesModel.Value.SetInteractionAll(false);
            return Transition.GoTo<WaitTurnSubstate>();
        }

        private void AddDisposables()
        {
            _onFieldSyncReceived.AddTo(Disposables);
            Disposables.Add(()=>InstanceFinder.ClientManager.UnregisterBroadcast<ClientFieldSync>(OnTurnSync));
            Disposables.Add(()=>InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurnTimeout>(OnTurnTimeout));
        }
    }
}