using System;
using System.Threading;
using Core.UI.Components;
using Cysharp.Threading.Tasks;
using Game.Entities;
using Game.Field;
using Game.UI;
using Game.User;
using UniState;
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
        private IEntityPlacementProjectionService _placementProjectionService;

        public InitialSubstate(
            IGameSubstatesInstaller gameSubstatesInstaller,
            FieldViewFactory fieldViewFactory,
            FieldGridFactory fieldGridFactory,
            EntitiesBackgroundFactory entitiesBackgroundFactory,
            EntitiesBackgroundGridFactory entitiesBackgroundGridFactory,
            EntityFactory entityFactory,
            AIUserRoundModel.Provider opponentModelProvider,
            UserRoundModel.Provider userModelProvider,
            UIRoundResultView.Factory uiRoundResultFactory,
            UIProvider<UIGame> uiProvider,
            IEntityPlacementProjectionService placementProjectionService)
        {
            _placementProjectionService = placementProjectionService;
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

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            var opponent = _opponentModelProvider.Model;
            var user = _userModelProvider.Model;
            
            var roundResultsViews = _uiProvider.UI.UIRoundResultViews;
            var userRoundResultViewModel = _uiRoundResultFactory.BindExisting(user, roundResultsViews[0]);
            var aiOpponentRoundResultViewModel = _uiRoundResultFactory.BindExisting(opponent, roundResultsViews[1]);

            var fieldView = await _fieldViewFactory.CreateAsync(token);
            await fieldView.PlaySquareBounceAllAxes(token);
            var fieldGrid =
                _fieldGridFactory.Create(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetPoints(fieldGrid);
            var fieldGridPaths = _fieldGridFactory.CreateCellPaths(fieldView.Collider, FieldConfig.FIELD_ROWS,
                FieldConfig.FIELD_COLUMNS);
            fieldView.DebugView.SetPaths(fieldGridPaths);

            _placementProjectionService.Initialize(fieldGrid,fieldGridPaths);

            var entitiesBackgroundView = await _entitiesBackgroundFactory.CreateAsync(token);
            var entitiesPositions = _entitiesBackgroundGridFactory.CreateOnRect(entitiesBackgroundView.Collider);
            
            Func<UniTask> userEntitiesBackgroundViewAnimation = ()=> 
                entitiesBackgroundView.PlayScaleFromCorner(new Vector2Int(1, 1),token);

            Func<UniTask<UserEntitiesModel>> userEntitiesModelCreation = ()=>
                _entityFactory.CreateAll(entitiesPositions, user.Owner, user.MaterialId, token);
           

            EntityPlacedModel[] prespawnPreset = null; //FieldConfig.CREATE_PRESPAWN_PRESET_1(opponentOwner);

            var opponentEntitiesBackgroundView =
                await _entitiesBackgroundFactory.CreateOpponentAsync(token);
            var opponentEntitiesPositions =
                _entitiesBackgroundGridFactory.CreateOnRect(opponentEntitiesBackgroundView.Collider, true);
            
            Func<UniTask> opponentEntitiesBackgroundViewAnimation = ()=> 
                opponentEntitiesBackgroundView.PlayScaleFromCorner(new Vector2Int(1, 0),token);

            Func<UniTask<UserEntitiesModel>> opponentEntitiesModelCreation = ()=>
                 _entityFactory.CreateAll(opponentEntitiesPositions, opponent.Owner, opponent.MaterialId, prespawnPreset,
                    fieldGrid, token);
            
            await UniTask.WhenAll(userEntitiesBackgroundViewAnimation(), opponentEntitiesBackgroundViewAnimation());
            
            var (userEntitiesModel, opponentEntitiesModel) = await UniTask.WhenAll(userEntitiesModelCreation(), opponentEntitiesModelCreation());
            
            opponentEntitiesModel.SetInteractionAll(false);
            
            var userEntitiesPlaceholder =
                await _entitiesBackgroundFactory.CreatePlaceholdersAsync(userEntitiesModel, token);
            
            var opponentEntitiesPlaceholder =
                await _entitiesBackgroundFactory.CreatePlaceholdersAsync(opponentEntitiesModel, token);
            
            

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
            
            return Transition.GoTo<ValidateSubstate>();
        }
    }
}