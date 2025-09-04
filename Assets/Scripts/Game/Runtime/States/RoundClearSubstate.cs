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
            [Inject(Id = AgentAIMoveSubstate.AGENT_MODEL_ID)] UserEntitiesModel opponentEntitiesModel,
            [Inject(Id = UserMoveSubstate.AGENT_MODEL_ID)] UserEntitiesModel userEntitiesModel,
            [Inject(Id = AgentAIMoveSubstate.AGENT_MODEL_ID)] EntitiesBackgroundView.EntitiesPlaceholderPresenter opponentEntitiesPlaceholder,
            [Inject(Id = UserMoveSubstate.AGENT_MODEL_ID)] EntitiesBackgroundView.EntitiesPlaceholderPresenter userEntitiesPlaceholder) :
            base(gameSubstateResolver)
        {
            _userEntitiesPlaceholder = userEntitiesPlaceholder;
            _opponentEntitiesPlaceholder = opponentEntitiesPlaceholder;
            _userEntitiesModel = userEntitiesModel;
            _opponentEntitiesModel = opponentEntitiesModel;
            _fieldModel = fieldModel;
        }

        public override async UniTask EnterAsync(CancellationToken ct)
        {
            Debug.Log("EndRoundSubstate: EnterAsync");
            _fieldModel.Drop();
            _opponentEntitiesModel.Drop();
            _userEntitiesModel.Drop();
            _opponentEntitiesPlaceholder.Drop();
            _userEntitiesPlaceholder.Drop();
            await SubstateMachine.ChangeStateAsync<ValidateSubstate>();
        }

        public override async UniTask ExitAsync(CancellationToken cancellationToken)
        {
            Debug.Log("EndRoundSubstate: ExitAsync");
        }
    }
}