using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.Entities;
using Game.Field;
using Game.States;
using Game.User;
using Multiplayer.Contracts;
using UniRx;
using UniState;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Client.States
{
    public class RoundClearSubstate : GameSubstate
    {
        private readonly LazyInject<FieldModel> _fieldModel;
        private readonly LazyInject<UserEntitiesModel> _opponentEntitiesModel;
        private readonly LazyInject<UserEntitiesModel> _userEntitiesModel;
        private readonly LazyInject<EntitiesBackgroundView.EntitiesPlaceholderPresenter> _opponentEntitiesPlaceholder;
        private readonly LazyInject<EntitiesBackgroundView.EntitiesPlaceholderPresenter> _userEntitiesPlaceholder;
        private readonly LazyInject<IStateProviderDebug> _stateProviderDebug;

        private ReactiveCommand<ClientTurn> _onTurnReceived;

        public RoundClearSubstate(
            LazyInject<FieldModel> fieldModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<UserEntitiesModel> opponentEntitiesModel,
            [Inject(Id = UserModelConfig.ID)] LazyInject<UserEntitiesModel> userEntitiesModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<EntitiesBackgroundView.EntitiesPlaceholderPresenter> opponentEntitiesPlaceholder,
            [Inject(Id = UserModelConfig.ID)] LazyInject<EntitiesBackgroundView.EntitiesPlaceholderPresenter> userEntitiesPlaceholder,
            [InjectOptional] LazyInject<IStateProviderDebug> stateProviderDebug)
        {
            _stateProviderDebug = stateProviderDebug;
            _userEntitiesPlaceholder = userEntitiesPlaceholder;
            _opponentEntitiesPlaceholder = opponentEntitiesPlaceholder;
            _userEntitiesModel = userEntitiesModel;
            _opponentEntitiesModel = opponentEntitiesModel;
            _fieldModel = fieldModel;

            _onTurnReceived = new ReactiveCommand<ClientTurn>();
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _stateProviderDebug?.Value?.ChangeState(this);
            AddDisposables();
            
            InstanceFinder.ClientManager.RegisterBroadcast<ClientTurn>(OnTurnReceived);

            _fieldModel.Value.Drop();
            _opponentEntitiesModel.Value.Drop();
            _userEntitiesModel.Value.Drop();
            _opponentEntitiesPlaceholder.Value.Drop();
            _userEntitiesPlaceholder.Value.Drop();
            
            InstanceFinder.ClientManager.Broadcast(new RoundResultResponse());

            var response = await _onTurnReceived.First()
                .ToUniTask(cancellationToken: token);
            
            return Transition.GoTo<TurnSubstate,ClientTurn>(response);
        }

        private void OnTurnReceived(ClientTurn arg1, Channel arg2)
        {
            _onTurnReceived?.Execute(arg1);
        }

        public override UniTask Exit(CancellationToken token)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurn>(OnTurnReceived);
            return base.Exit(token);
        }

        private void AddDisposables()
        {
            Disposables.Add(() => InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurn>(OnTurnReceived));
            _onTurnReceived.AddTo(Disposables);
        }
    }
}