using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;
using Zenject;

namespace Game.States
{
    public class ValidateSubstate : GameSubstate
    {
        private int _passedRounds;
        private IActiveUserProvider _activeUserProvider;
        private FieldModel _fieldModel;
        private UserEntitiesModel _userEntitiesModel;
        private UserEntitiesModel _botEntitiesModel;


        public ValidateSubstate(
            [Inject(Id = UserModelConfig.OPPONENT_ID)] UserEntitiesModel botEntitiesModel,
            [Inject(Id = UserModelConfig.ID)] UserEntitiesModel userEntitiesModel,
            IGameSubstateResolver gameSubstateResolver,
            IActiveUserProvider activeUserProvider,
            FieldModel fieldModel) 
            : base(gameSubstateResolver)
        {
            _botEntitiesModel = botEntitiesModel;
            _userEntitiesModel = userEntitiesModel;
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
            
            var activeUser = _activeUserProvider.GetActiveUserId();
            var activeUserEntitiesModel =
                _userEntitiesModel.Owner == activeUser ? _userEntitiesModel : _botEntitiesModel;
            var isDraw = _fieldModel.IsDraw(activeUserEntitiesModel);
            if (isDraw)
            {
                _passedRounds++;
                if (_passedRounds >= FieldConfig.ROUNDS_AMOUNT)
                {
                    await SubstateMachine.ChangeStateAsync<FinalRoundResultSubstateGameSubstate, FinalRoundResultSubstateGameSubstate.Payload>(
                        new FinalRoundResultSubstateGameSubstate.Payload(_userEntitiesModel.Owner,_botEntitiesModel.Owner),ct);
                    return;
                }
                await SubstateMachine.ChangeStateAsync<RoundResultSubstate, RoundResultSubstate.Payload>(
                    new RoundResultSubstate.Payload(_userEntitiesModel.Owner,_botEntitiesModel.Owner),ct);
                return;
            }
            
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

        public override UniTask ExitAsync(CancellationToken ct)
        {
            // No specific exit logic needed for validation
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            
        }
    }
}