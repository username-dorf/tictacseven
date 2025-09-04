using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.Entities;
using Game.Field;
using Game.User;
using UnityEngine;
using Zenject;

namespace Game.States
{
    public class RoundClearSubstate : GameSubstate
    {
        private readonly FieldModel _fieldModel;
        private readonly UserEntitiesModel _opponentEntitiesModel;
        private readonly UserEntitiesModel _userEntitiesModel;
        private readonly EntitiesBackgroundView.EntitiesPlaceholderPresenter _opponentEntitiesPlaceholder;
        private readonly EntitiesBackgroundView.EntitiesPlaceholderPresenter _userEntitiesPlaceholder;

        public RoundClearSubstate(
            IGameSubstateResolver gameSubstateResolver,
            FieldModel fieldModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] UserEntitiesModel opponentEntitiesModel,
            [Inject(Id = UserModelConfig.ID)] UserEntitiesModel userEntitiesModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] EntitiesBackgroundView.EntitiesPlaceholderPresenter opponentEntitiesPlaceholder,
            [Inject(Id = UserModelConfig.ID)] EntitiesBackgroundView.EntitiesPlaceholderPresenter userEntitiesPlaceholder) 
            : base(gameSubstateResolver)
        {
            _userEntitiesPlaceholder = userEntitiesPlaceholder;
            _opponentEntitiesPlaceholder = opponentEntitiesPlaceholder;
            _userEntitiesModel = userEntitiesModel;
            _opponentEntitiesModel = opponentEntitiesModel;
            _fieldModel = fieldModel;
        }

        public override async UniTask EnterAsync(CancellationToken ct)
        {
            _fieldModel.Drop();
            _opponentEntitiesModel.Drop();
            _userEntitiesModel.Drop();
            _opponentEntitiesPlaceholder.Drop();
            _userEntitiesPlaceholder.Drop();
            await SubstateMachine.ChangeStateAsync<ValidateSubstate>(ct);
        }

        public override async UniTask ExitAsync(CancellationToken ct)
        {
        }

        public override void Dispose()
        {
            
        }
    }
}