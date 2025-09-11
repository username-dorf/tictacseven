using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;
using UniRx;
using UniState;
using Zenject;

namespace Game.States
{
    public class UserMoveSubstate : GameSubstate
    {
        
        private readonly LazyInject<FieldModel> _field;
        private readonly LazyInject<UserEntitiesModel> _userEntitiesModel;
        private readonly LazyInject<UserRoundModel.Provider> _userRoundModelProvider;

        public UserMoveSubstate(
            LazyInject<FieldModel> field,
            [Inject(Id = UserModelConfig.ID)] LazyInject<UserEntitiesModel> userEntitiesModel,
            LazyInject<UserRoundModel.Provider> userRoundModelProvider)
        {
            _userRoundModelProvider = userRoundModelProvider;
            _userEntitiesModel = userEntitiesModel;
            _field = field;
        }
        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _userEntitiesModel.Value.SetInteractionAll(true);
            _userRoundModelProvider.Value.Model.SetAwaitingTurn(true);
            
            await _field.Value.OnEntityChanged
                .First()
                .ToUniTask(cancellationToken: token);

            return Transition.GoTo<ValidateSubstate>();
        }

        public override UniTask Exit(CancellationToken token)
        {
            _userEntitiesModel.Value.SetInteractionAll(false);
            _userRoundModelProvider.Value.Model.SetAwaitingTurn(false);
            return base.Exit(token);
        }
    }
}