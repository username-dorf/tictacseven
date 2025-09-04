using System;
using System.Threading;
using Core.UI.Components;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.Entities;
using Game.Field;
using Game.States;
using Game.UI;
using Game.User;
using Multiplayer.Contracts;
using UnityEngine;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Client.States
{
    public class InitialSubstate : GameSubstate<InitialSubstate.Payload>
    {
        public struct Payload
        {
            public int Owner;
            public int OpponentOwner;
            public UserPreferencesDto OpponentPreferences;
        }
        
        private UIProvider<UIGame> _uiProvider;
        private UIRoundResultView.Factory _uiRoundResultFactory;
        private UserRoundModel.Provider _userModelProvider;
        
        private FieldViewFactory _fieldViewFactory;
        private FieldGridFactory _fieldGridFactory;
        private EntitiesBackgroundFactory _entitiesBackgroundFactory;
        private EntitiesBackgroundGridFactory _entitiesBackgroundGridFactory;
        private EntityFactory _entityFactory;
        private IGameSubstatesInstaller _substateInstaller;

        public InitialSubstate(
            FieldViewFactory fieldViewFactory,
            FieldGridFactory fieldGridFactory,
            EntitiesBackgroundFactory entitiesBackgroundFactory,
            EntitiesBackgroundGridFactory entitiesBackgroundGridFactory,
            EntityFactory entityFactory,
            UserRoundModel.Provider userModelProvider,
            UIProvider<UIGame> uiProvider,
            UIRoundResultView.Factory uiRoundResultFactory,
            IGameSubstateResolver substateResolverFactory,
            IGameSubstatesInstaller substateInstaller) 
            : base(substateResolverFactory)
        {
            _entityFactory = entityFactory;
            _entitiesBackgroundGridFactory = entitiesBackgroundGridFactory;
            _entitiesBackgroundFactory = entitiesBackgroundFactory;
            _fieldGridFactory = fieldGridFactory;
            _fieldViewFactory = fieldViewFactory;
            _userModelProvider = userModelProvider;
            _uiRoundResultFactory = uiRoundResultFactory;
            _uiProvider = uiProvider;
            _substateInstaller = substateInstaller;
        }

        protected override async UniTask EnterAsync(InitialSubstate.Payload payload, CancellationToken ct)
        {
            var user = _userModelProvider.Model;
            user.SetOwner(payload.Owner);
            
            var opponent = new ClientRoundModel(payload.OpponentPreferences);
            opponent.SetOwner(payload.OpponentOwner);
            
            var roundResultsViews = _uiProvider.UI.UIRoundResultViews;
            var userRoundResultViewModel = _uiRoundResultFactory.BindExisting(user, roundResultsViews[0]);
            var opponentRoundResultViewModel = _uiRoundResultFactory.BindExisting(opponent, roundResultsViews[1]);
            
            var fieldView = await _fieldViewFactory.CreateAsync(ct);
            await fieldView.PlaySquareBounceAllAxes(ct);
            
            var fieldGrid =
                _fieldGridFactory.Create(fieldView.Collider, FieldConfig.FIELD_ROWS, FieldConfig.FIELD_COLUMNS);

            var entitiesBackgroundView = await _entitiesBackgroundFactory.CreateAsync(ct);
            var entitiesPositions = _entitiesBackgroundGridFactory.CreateOnRect(entitiesBackgroundView.Collider);
            
            Func<UniTask> userEntitiesBackgroundViewAnimation = ()=> 
                entitiesBackgroundView.PlayScaleFromCorner(new Vector2Int(1, 1),ct);

            Func<UniTask<UserEntitiesModel>> userEntitiesModelCreation = ()=>
                _entityFactory.CreateAll(entitiesPositions, user.Owner, user.MaterialId, ct);
            
            var opponentEntitiesBackgroundView =
                await _entitiesBackgroundFactory.CreateOpponentAsync(ct);
            var opponentEntitiesPositions =
                _entitiesBackgroundGridFactory.CreateOnRect(opponentEntitiesBackgroundView.Collider, true);
            
            Func<UniTask> opponentEntitiesBackgroundViewAnimation = ()=> 
                opponentEntitiesBackgroundView.PlayScaleFromCorner(new Vector2Int(1, 0),ct);

            Func<UniTask<UserEntitiesModel>> opponentEntitiesModelCreation = ()=>
                 _entityFactory.CreateAll(opponentEntitiesPositions, opponent.Owner, opponent.MaterialId, null,
                    fieldGrid, ct);
            
            await UniTask.WhenAll(userEntitiesBackgroundViewAnimation(), opponentEntitiesBackgroundViewAnimation());
            
            var (userEntitiesModel, opponentEntitiesModel) = await UniTask.WhenAll(userEntitiesModelCreation(), opponentEntitiesModelCreation());
            
            opponentEntitiesModel.SetInteractionAll(false);
            
            var userEntitiesPlaceholder =
                await _entitiesBackgroundFactory.CreatePlaceholdersAsync(userEntitiesModel, ct);
            
            var opponentEntitiesPlaceholder =
                await _entitiesBackgroundFactory.CreatePlaceholdersAsync(opponentEntitiesModel, ct);
            
            

            var fieldModel = new FieldModel(fieldGrid);
            var fieldViewModel = new FieldViewModel(fieldModel, userEntitiesModel, opponentEntitiesModel);

            _substateInstaller
                .BindFieldModel(fieldModel)
                .BindEntitiesModel(opponentEntitiesModel, UserModelConfig.OPPONENT_ID)
                .BindPlaceholderPresenter(userEntitiesPlaceholder, UserModelConfig.ID)
                .BindEntitiesModel(userEntitiesModel, UserModelConfig.ID)
                .BindPlaceholderPresenter(opponentEntitiesPlaceholder, UserModelConfig.OPPONENT_ID)
                .BindUserRoundModel(opponent,UserModelConfig.OPPONENT_ID)
                .BindUserRoundModel(user, UserModelConfig.ID)
                .Build();

            
            InstanceFinder.ClientManager.Broadcast(new ClientInitializationResponse {
                Accepted = true
            });
        }

        public override UniTask ExitAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
        }
    }
}