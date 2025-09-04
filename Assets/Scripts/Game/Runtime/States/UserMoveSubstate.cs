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
        public const string AGENT_MODEL_ID = "UserModel";
        
        private readonly FieldModel _field;
        private readonly UserEntitiesModel _userEntitiesModel;
        private CompositeDisposable _disposable;

        public UserMoveSubstate(
            IGameSubstateResolver gameSubstateResolver,
            FieldModel field,
            [Inject(Id = UserMoveSubstate.AGENT_MODEL_ID)] UserEntitiesModel userEntitiesModel) : base(gameSubstateResolver)
        {
            _disposable = new CompositeDisposable();
            _userEntitiesModel = userEntitiesModel;
            _field = field;
        }
        public override async UniTask EnterAsync(CancellationToken ct)
        {
            Debug.Log("UserMoveSubstate: EnterAsync");
            _userEntitiesModel.SetInteractionAll(true);
            _field.OnEntityChanged
                .Subscribe(_=>SubstateMachine.ChangeStateAsync<ValidateSubstate>())
                .AddTo(_disposable);
        }

        public override async UniTask ExitAsync(CancellationToken _)
        {
            Debug.Log("UserMoveSubstate: ExitAsync");
            _disposable?.Clear();
            _userEntitiesModel.SetInteractionAll(false);
        }
    }
}