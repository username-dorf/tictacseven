using System.Threading;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.Entities;
using Game.Field;
using Game.States;
using Game.User;
using Multiplayer.Contracts;
using UniRx;
using UnityEngine;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Client.States
{
    public class TurnSubstate: GameSubstate<ClientTurn>
    {
        private CompositeDisposable _disposable;
        private FieldModel _fieldModel;
        private UserRoundModel _userRoundModel;
        private IUserPreferencesProvider _userPreferencesProvider;
        private UserEntitiesModel _userEntitiesModel;
        private UserRoundModel _opponentRoundModel;

        public TurnSubstate(
            FieldModel fieldModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] UserRoundModel opponentRoundModel,
            [Inject(Id = UserModelConfig.ID)] UserRoundModel userRoundModel,
            [Inject(Id = UserModelConfig.ID)] UserEntitiesModel userEntitiesModel,
            IUserPreferencesProvider userPreferencesProvider,
            IGameSubstateResolver substateResolverFactory) : base(substateResolverFactory)
        {
            _opponentRoundModel = opponentRoundModel;
            _userEntitiesModel = userEntitiesModel;
            _userPreferencesProvider = userPreferencesProvider;
            _userRoundModel = userRoundModel;
            _fieldModel = fieldModel;
            _disposable = new CompositeDisposable();
        }

        protected override async UniTask EnterAsync(ClientTurn payload, CancellationToken ct)
        {
            var isCurrentClientActive = _userPreferencesProvider.Current.User.Id == payload.ActiveClientId;
            _userRoundModel.SetAwaitingTurn(isCurrentClientActive);
            _opponentRoundModel.SetAwaitingTurn(!isCurrentClientActive);

            _userEntitiesModel.SetInteractionAll(isCurrentClientActive);
            if (!isCurrentClientActive)
            {
                SubstateMachine.ChangeStateAsync<ServerSyncSubstate>(CancellationToken.None)
                    .Forget();
                return;
            }

            InstanceFinder.ClientManager.RegisterBroadcast<ClientTurnTimeout>(OnTurnTimeout);

            _fieldModel.OnEntityChanged
                .Subscribe(OnTurnDone)
                .AddTo(_disposable);
        }

        public override UniTask ExitAsync(CancellationToken ct)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurnTimeout>(OnTurnTimeout);
            _disposable?.Clear();
            return UniTask.CompletedTask;
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
            _userEntitiesModel.SetInteractionAll(false);
            
            SubstateMachine.ChangeStateAsync<ServerSyncSubstate>(CancellationToken.None)
                .Forget();
        }

        private void OnTurnTimeout(ClientTurnTimeout request, Channel channel)
        {
            SubstateMachine.ChangeStateAsync<ServerSyncSubstate>(CancellationToken.None)
                .Forget();
            
            _userEntitiesModel.SetInteractionAll(false);
        }

        public override void Dispose()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurnTimeout>(OnTurnTimeout);
            _disposable?.Dispose();
        }
    }
}