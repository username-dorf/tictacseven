using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.Entities;
using Game.Field;
using Game.User;
using UniState;
using UnityEngine;
using Zenject;

namespace Game.States
{
    public class RoundClearSubstate : GameSubstate
    {
        private readonly LazyInject<FieldModel> _fieldModel;
        private readonly LazyInject<UserEntitiesModel> _opponentEntitiesModel;
        private readonly LazyInject<UserEntitiesModel> _userEntitiesModel;
        private readonly LazyInject<EntitiesBackgroundView.EntitiesPlaceholderPresenter> _opponentEntitiesPlaceholder;
        private readonly LazyInject<EntitiesBackgroundView.EntitiesPlaceholderPresenter> _userEntitiesPlaceholder;

        public RoundClearSubstate(
            LazyInject<FieldModel> fieldModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<UserEntitiesModel> opponentEntitiesModel,
            [Inject(Id = UserModelConfig.ID)] LazyInject<UserEntitiesModel> userEntitiesModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<EntitiesBackgroundView.EntitiesPlaceholderPresenter> opponentEntitiesPlaceholder,
            [Inject(Id = UserModelConfig.ID)] LazyInject<EntitiesBackgroundView.EntitiesPlaceholderPresenter> userEntitiesPlaceholder)
        {
            _userEntitiesPlaceholder = userEntitiesPlaceholder;
            _opponentEntitiesPlaceholder = opponentEntitiesPlaceholder;
            _userEntitiesModel = userEntitiesModel;
            _opponentEntitiesModel = opponentEntitiesModel;
            _fieldModel = fieldModel;
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _fieldModel.Value.Drop();
            _opponentEntitiesModel.Value.Drop();
            _userEntitiesModel.Value.Drop();
            _opponentEntitiesPlaceholder.Value.Drop();
            _userEntitiesPlaceholder.Value.Drop();
            return Transition.GoTo<ValidateSubstate>();
        }
    }
}