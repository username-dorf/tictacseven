using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Entities;
using Game.Field;
using Game.User;
using UnityEngine;

namespace Game.States
{
    
    public class InitialSubstate : GameSubstate
    {
        private FieldViewFactory _fieldViewFactory;
        private FieldGridFactory _fieldGridFactory;
        private EntitiesBackgroundFactory _entitiesBackgroundFactory;
        private EntitiesBackgroundGridFactory _entitiesBackgroundGridFactory;
        private EntityFactory _entityFactory;
        private IGameSubstatesInstaller _gameSubstatesInstaller;
        private AIUserRoundModel.Provider _opponentModelProvider;
        private UserRoundModel.Provider _userModelProvider;

        public InitialSubstate(
            IGameSubstatesInstaller gameSubstatesInstaller,
            IGameSubstateResolver substateResolverFactory,
            FieldViewFactory fieldViewFactory,
            FieldGridFactory fieldGridFactory,
            EntitiesBackgroundFactory entitiesBackgroundFactory,
            EntitiesBackgroundGridFactory entitiesBackgroundGridFactory,
            EntityFactory entityFactory,
            AIUserRoundModel.Provider opponentModelProvider,
            UserRoundModel.Provider userModelProvider) : base(substateResolverFactory)
        {
            _userModelProvider = userModelProvider;
            _opponentModelProvider = opponentModelProvider;
            _gameSubstatesInstaller = gameSubstatesInstaller;
            _entityFactory = entityFactory;
            _entitiesBackgroundGridFactory = entitiesBackgroundGridFactory;
            _entitiesBackgroundFactory = entitiesBackgroundFactory;
            _fieldGridFactory = fieldGridFactory;
            _fieldViewFactory = fieldViewFactory;
        }

        public override async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            Debug.Log("InitialSubstate: EnterAsync");
            var opponent = _opponentModelProvider.Model;
            var user = _userModelProvider.Model;

            var fieldView = await _fieldViewFactory.CreateAsync(cancellationToken);
            var fieldGrid =
                _fieldGridFactory.Create(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetPoints(fieldGrid);

            var debugMatrixLines =
                _fieldGridFactory.CreateLines(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetLines(debugMatrixLines);

            var entitiesBackgroundView = await _entitiesBackgroundFactory.CreateAsync(cancellationToken);
            var entitiesPositions = _entitiesBackgroundGridFactory.CreateOnRect(entitiesBackgroundView.Collider);
            entitiesBackgroundView.DebugView.SetPoints(entitiesPositions);

            var userEntitiesModel = await
                _entityFactory.CreateAll(entitiesPositions, user.Owner, cancellationToken);
            var userEntitiesPlaceholder =
                await _entitiesBackgroundFactory.CreatePlaceholdersAsync(userEntitiesModel, cancellationToken);

            EntityPlacedModel[] prespawnPreset = null; //FieldConfig.CREATE_PRESPAWN_PRESET_1(opponentOwner);

            var opponentEntitiesBackgroundView =
                await _entitiesBackgroundFactory.CreateOpponentAsync(cancellationToken);
            var opponentEntitiesPositions =
                _entitiesBackgroundGridFactory.CreateOnRect(opponentEntitiesBackgroundView.Collider, true);
            var opponentEntitiesModel =
                await _entityFactory.CreateAll(opponentEntitiesPositions, opponent.Owner, prespawnPreset,
                    fieldGrid, cancellationToken);
            opponentEntitiesModel.SetInteractionAll(false);
            var opponentEntitiesPlaceholder =
                await _entitiesBackgroundFactory.CreatePlaceholdersAsync(opponentEntitiesModel, cancellationToken);


            var fieldModel = new FieldModel(fieldGrid, prespawnPreset);
            var fieldViewModel = new FieldViewModel(fieldModel, userEntitiesModel, opponentEntitiesModel);

            _gameSubstatesInstaller
                .BindFieldModel(fieldModel)
                .BindEntitiesModel(opponentEntitiesModel, AgentAIMoveSubstate.AGENT_MODEL_ID)
                .BindPlaceholderPresenter(userEntitiesPlaceholder, UserMoveSubstate.AGENT_MODEL_ID)
                .BindEntitiesModel(userEntitiesModel, UserMoveSubstate.AGENT_MODEL_ID)
                .BindPlaceholderPresenter(opponentEntitiesPlaceholder, AgentAIMoveSubstate.AGENT_MODEL_ID)
                .BindUserRoundModel(opponent,AgentAIMoveSubstate.AGENT_MODEL_ID)
                .BindUserRoundModel(user, UserMoveSubstate.AGENT_MODEL_ID)
                .Build();

            await SubstateMachine.ChangeStateAsync<ValidateSubstate>();
        }

        public override async UniTask ExitAsync(CancellationToken cancellationToken)
        {
            Debug.Log("InitialSubstate: ExitAsync");
        }
    }
}