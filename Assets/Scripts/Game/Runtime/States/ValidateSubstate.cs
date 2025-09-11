using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;
using UniState;
using Zenject;

namespace Game.States
{
    public class ValidateSubstate : GameSubstate
    {
        private int _passedRounds;
        private LazyInject<IActiveUserProvider> _activeUserProvider;
        private LazyInject<FieldModel> _fieldModel;
        private LazyInject<UserEntitiesModel> _userEntitiesModel;
        private LazyInject<UserEntitiesModel> _botEntitiesModel;


        public ValidateSubstate(
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<UserEntitiesModel> botEntitiesModel,
            [Inject(Id = UserModelConfig.ID)] LazyInject<UserEntitiesModel> userEntitiesModel,
            LazyInject<IActiveUserProvider> activeUserProvider,
            LazyInject<FieldModel> fieldModel)
        {
            _botEntitiesModel = botEntitiesModel;
            _userEntitiesModel = userEntitiesModel;
            _fieldModel = fieldModel;
            _activeUserProvider = activeUserProvider;
        }
        public override async UniTask<StateTransitionInfo> Execute(CancellationToken ct)
        {
            await UniTask.WaitForEndOfFrame(ct);
            
            var winner = _fieldModel.Value.GetWinner();
            if (winner.HasValue)
            {
                _passedRounds++;
                if (_passedRounds >= FieldConfig.ROUNDS_AMOUNT)
                {
                    return Transition.GoTo<FinalRoundResultSubstateGameSubstate, FinalRoundResultSubstateGameSubstate.PayloadModel>(
                        new FinalRoundResultSubstateGameSubstate.PayloadModel(winner.Value));
                }
                return Transition.GoTo<RoundResultSubstate, RoundResultSubstate.PayloadModel>(
                    new RoundResultSubstate.PayloadModel(winner.Value));
            }
            
            _activeUserProvider.Value.ChangeNextUser();
            
            var activeUser = _activeUserProvider.Value.GetActiveUserId();
            var activeUserEntitiesModel =
                _userEntitiesModel.Value.Owner == activeUser ? _userEntitiesModel : _botEntitiesModel;
            var isDraw = _fieldModel.Value.IsDraw(activeUserEntitiesModel.Value);
            if (isDraw)
            {
                _passedRounds++;
                if (_passedRounds >= FieldConfig.ROUNDS_AMOUNT)
                {
                    return Transition.GoTo<FinalRoundResultSubstateGameSubstate, FinalRoundResultSubstateGameSubstate.PayloadModel>(
                        new FinalRoundResultSubstateGameSubstate.PayloadModel(_userEntitiesModel.Value.Owner,_botEntitiesModel.Value.Owner));
                }
                return Transition.GoTo<RoundResultSubstate, RoundResultSubstate.PayloadModel>(
                    new RoundResultSubstate.PayloadModel(_userEntitiesModel.Value.Owner,_botEntitiesModel.Value.Owner));
            }
            
            var activeUserId = _activeUserProvider.Value.GetActiveUserId();
            return activeUserId==2 ? Transition.GoTo<UserMoveSubstate>() : Transition.GoTo<AgentAIMoveSubstate>();
        }
    }
}