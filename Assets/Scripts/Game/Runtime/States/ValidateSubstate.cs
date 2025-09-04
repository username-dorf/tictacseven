using System.Collections.Generic;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;
using UnityEngine;
using Zenject;

namespace Game.States
{
    public class ValidateSubstate : GameSubstate
    {
        private IActiveUserProvider _activeUserProvider;
        private FieldModel _fieldModel;


        public ValidateSubstate(
            IGameSubstateResolver gameSubstateResolver,
            IActiveUserProvider activeUserProvider,
            FieldModel fieldModel) 
            : base(gameSubstateResolver)
        {
            _fieldModel = fieldModel;
            _activeUserProvider = activeUserProvider;
        }
        public override async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            Debug.Log("ValidateSubstate: EnterAsync");
            await UniTask.WaitForEndOfFrame(cancellationToken);
            var winner = _fieldModel.GetWinner();
            if (winner.HasValue)
            {
                await SubstateMachine.ChangeStateAsync<RoundResultSubstate, RoundResultSubstate.Payload>(
                    new RoundResultSubstate.Payload(winner.Value));
                return;
            }
            _activeUserProvider.ChangeNextUser();
            var activeUserId = _activeUserProvider.GetActiveUserId();
            if (activeUserId==2)
            {
                await SubstateMachine.ChangeStateAsync<UserMoveSubstate>();
            }
            else
            {
                await SubstateMachine.ChangeStateAsync<AgentAIMoveSubstate>();
            }
        }

        public override UniTask ExitAsync(CancellationToken cancellationToken)
        {
            Debug.Log("ValidateSubstate: ExitAsync");
            // No specific exit logic needed for validation
            return UniTask.CompletedTask;
        }
    }
}