using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;

namespace Game.States
{
    public class ValidateSubstate : GameSubstate
    {
        private int _passedRounds;
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
        public override async UniTask EnterAsync(CancellationToken ct)
        {
            await UniTask.WaitForEndOfFrame(ct);
            var winner = _fieldModel.GetWinner();
            if (winner.HasValue)
            {
                _passedRounds++;
                if (_passedRounds >= FieldConfig.ROUNDS_AMOUNT)
                {
                    await SubstateMachine.ChangeStateAsync<FinalRoundResultSubstateGameSubstate, FinalRoundResultSubstateGameSubstate.Payload>(
                        new FinalRoundResultSubstateGameSubstate.Payload(winner.Value),ct);
                    return;
                }
                await SubstateMachine.ChangeStateAsync<RoundResultSubstate, RoundResultSubstate.Payload>(
                    new RoundResultSubstate.Payload(winner.Value),ct);
                return;
            }
            _activeUserProvider.ChangeNextUser();
            var activeUserId = _activeUserProvider.GetActiveUserId();
            if (activeUserId==2)
            {
                await SubstateMachine.ChangeStateAsync<UserMoveSubstate>(ct);
            }
            else
            {
                await SubstateMachine.ChangeStateAsync<AgentAIMoveSubstate>(ct);
            }
        }

        public override UniTask ExitAsync(CancellationToken cancellationToken)
        {
            // No specific exit logic needed for validation
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            
        }
    }
}