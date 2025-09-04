using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.Entities;
using Game.Field;
using Game.States;
using Game.User;
using Multiplayer.Contracts;
using Zenject;

namespace Multiplayer.Client.States
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
            [Inject(Id = UserModelConfig.OPPONENT_ID)]
            UserEntitiesModel opponentEntitiesModel,
            [Inject(Id = UserModelConfig.ID)] UserEntitiesModel userEntitiesModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)]
            EntitiesBackgroundView.EntitiesPlaceholderPresenter opponentEntitiesPlaceholder,
            [Inject(Id = UserModelConfig.ID)]
            EntitiesBackgroundView.EntitiesPlaceholderPresenter userEntitiesPlaceholder)
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
            InstanceFinder.ClientManager.Broadcast(new RoundResultResponse());
        }

        public override async UniTask ExitAsync(CancellationToken ct)
        {
        }

        public override void Dispose()
        {

        }
    }
}