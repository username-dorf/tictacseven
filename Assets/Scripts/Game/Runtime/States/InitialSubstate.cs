using System;
using System.Threading;
using Core.UI.Components;
using Cysharp.Threading.Tasks;
using Game.Entities;
using Game.Field;
using Game.UI;
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
        
        private UIRoundResultView.Factory _uiRoundResultFactory;
        private UIProvider<UIGame> _uiProvider;

        public InitialSubstate(
            IGameSubstatesInstaller gameSubstatesInstaller,
            IGameSubstateResolver substateResolverFactory,
            FieldViewFactory fieldViewFactory,
            FieldGridFactory fieldGridFactory,
            EntitiesBackgroundFactory entitiesBackgroundFactory,
            EntitiesBackgroundGridFactory entitiesBackgroundGridFactory,
            EntityFactory entityFactory,
            AIUserRoundModel.Provider opponentModelProvider,
            UserRoundModel.Provider userModelProvider,
            UIRoundResultView.Factory uiRoundResultFactory,
            UIProvider<UIGame> uiProvider) : base(substateResolverFactory)
        {
            _uiProvider = uiProvider;
            _uiRoundResultFactory = uiRoundResultFactory;
            _userModelProvider = userModelProvider;
            _opponentModelProvider = opponentModelProvider;
            _gameSubstatesInstaller = gameSubstatesInstaller;
            _entityFactory = entityFactory;
            _entitiesBackgroundGridFactory = entitiesBackgroundGridFactory;
            _entitiesBackgroundFactory = entitiesBackgroundFactory;
            _fieldGridFactory = fieldGridFactory;
            _fieldViewFactory = fieldViewFactory;
        }

        public override async UniTask EnterAsync(CancellationToken ct)
        {
            Debug.Log("Enter async initial state");
            var opponent = _opponentModelProvider.Model;
            var user = _userModelProvider.Model;
            
            var roundResultsViews = _uiProvider.UI.UIRoundResultViews;
            var userRoundResultViewModel = _uiRoundResultFactory.BindExisting(user, roundResultsViews[0]);
            var aiOpponentRoundResultViewModel = _uiRoundResultFactory.BindExisting(opponent, roundResultsViews[1]);

            var fieldView = await _fieldViewFactory.CreateAsync(ct);
            await fieldView.PlaySquareBounceAllAxes(ct);
            var fieldGrid =
                _fieldGridFactory.Create(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetPoints(fieldGrid);

            var entitiesBackgroundView = await _entitiesBackgroundFactory.CreateAsync(ct);
            var entitiesPositions = _entitiesBackgroundGridFactory.CreateOnRect(entitiesBackgroundView.Collider);
            
            Func<UniTask> userEntitiesBackgroundViewAnimation = ()=> 
                entitiesBackgroundView.PlayScaleFromCorner(new Vector2Int(1, 1),ct);

            Func<UniTask<UserEntitiesModel>> userEntitiesModelCreation = ()=>
                _entityFactory.CreateAll(entitiesPositions, user.Owner, user.MaterialId, ct);
           

            EntityPlacedModel[] prespawnPreset = null; //FieldConfig.CREATE_PRESPAWN_PRESET_1(opponentOwner);

            var opponentEntitiesBackgroundView =
                await _entitiesBackgroundFactory.CreateOpponentAsync(ct);
            var opponentEntitiesPositions =
                _entitiesBackgroundGridFactory.CreateOnRect(opponentEntitiesBackgroundView.Collider, true);
            
            Func<UniTask> opponentEntitiesBackgroundViewAnimation = ()=> 
                opponentEntitiesBackgroundView.PlayScaleFromCorner(new Vector2Int(1, 0),ct);

            Func<UniTask<UserEntitiesModel>> opponentEntitiesModelCreation = ()=>
                 _entityFactory.CreateAll(opponentEntitiesPositions, opponent.Owner, opponent.MaterialId, prespawnPreset,
                    fieldGrid, ct);
            
            await UniTask.WhenAll(userEntitiesBackgroundViewAnimation(), opponentEntitiesBackgroundViewAnimation());
            
            var (userEntitiesModel, opponentEntitiesModel) = await UniTask.WhenAll(userEntitiesModelCreation(), opponentEntitiesModelCreation());
            
            opponentEntitiesModel.SetInteractionAll(false);
            
            var userEntitiesPlaceholder =
                await _entitiesBackgroundFactory.CreatePlaceholdersAsync(userEntitiesModel, ct);
            
            var opponentEntitiesPlaceholder =
                await _entitiesBackgroundFactory.CreatePlaceholdersAsync(opponentEntitiesModel, ct);
            
            

            var fieldModel = new FieldModel(fieldGrid, prespawnPreset);
            var fieldViewModel = new FieldViewModel(fieldModel, userEntitiesModel, opponentEntitiesModel);

            _gameSubstatesInstaller
                .BindFieldModel(fieldModel)
                .BindEntitiesModel(opponentEntitiesModel, UserModelConfig.OPPONENT_ID)
                .BindPlaceholderPresenter(userEntitiesPlaceholder, UserModelConfig.ID)
                .BindEntitiesModel(userEntitiesModel, UserModelConfig.ID)
                .BindPlaceholderPresenter(opponentEntitiesPlaceholder, UserModelConfig.OPPONENT_ID)
                .BindUserRoundModel(opponent,UserModelConfig.OPPONENT_ID)
                .BindUserRoundModel(user, UserModelConfig.ID)
                .Build();
            
            await SubstateMachine.ChangeStateAsync<ValidateSubstate>(ct);
        }

        public override async UniTask ExitAsync(CancellationToken ct)
        {
        }

        public override void Dispose()
        {
            
        }
    }
}