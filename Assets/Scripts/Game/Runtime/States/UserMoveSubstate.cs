using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.States
{
    public class UserMoveSubstate : GameSubstate
    {
        
        private readonly FieldModel _field;
        private readonly UserEntitiesModel _userEntitiesModel;
        private CompositeDisposable _disposable;
        private UserRoundModel.Provider _userRoundModelProvider;

        public UserMoveSubstate(
            IGameSubstateResolver gameSubstateResolver,
            FieldModel field,
            [Inject(Id = UserModelConfig.ID)] UserEntitiesModel userEntitiesModel,
            UserRoundModel.Provider userRoundModelProvider) : base(gameSubstateResolver)
        {
            _userRoundModelProvider = userRoundModelProvider;
            _disposable = new CompositeDisposable();
            _userEntitiesModel = userEntitiesModel;
            _field = field;
        }
        public override async UniTask EnterAsync(CancellationToken ct)
        {
            _userEntitiesModel.SetInteractionAll(true);
            _userRoundModelProvider.Model.SetAwaitingTurn(true);
            _field.OnEntityChanged
                .Subscribe(_=>SubstateMachine.ChangeStateAsync<ValidateSubstate>(ct))
                .AddTo(_disposable);
            
        }

        public override async UniTask ExitAsync(CancellationToken ct)
        {
            _disposable?.Clear();
            _userEntitiesModel.SetInteractionAll(false);
            _userRoundModelProvider.Model.SetAwaitingTurn(false);

        }

        public override void Dispose()
        {
            
        }
    }
}