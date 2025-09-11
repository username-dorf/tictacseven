using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;
using UniState;
using UnityEngine;
using Zenject;

namespace Game.States
{
    public class AgentAIMoveSubstate : GameSubstate
    {
        private LazyInject<FieldModel> _fieldModel;
        private LazyInject<UserEntitiesModel> _botEntitiesModel;
        private UserEntitiesController _controller;
        private LazyInject<UserEntitiesModel> _userEntitiesModel;
        private AgentAIController _agentAIController;
        private AgentThinkingAIController _agentThinkingAIController;
        private FieldViewProvider _fieldViewProvider;
        private LazyInject<AIUserRoundModel.Provider> _userRoundModelProvider;

        public AgentAIMoveSubstate(
            LazyInject<FieldModel> fieldModel,
            FieldViewProvider fieldViewProvider,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<UserEntitiesModel> botEntitiesModel,
            [Inject(Id = UserModelConfig.ID)] LazyInject<UserEntitiesModel> userEntitiesModel,
            AgentAIController agentAIController,
            AgentThinkingAIController agentThinkingAIController,
            LazyInject<AIUserRoundModel.Provider> userRoundModelProvider)
        {
            _userRoundModelProvider = userRoundModelProvider;
            _fieldViewProvider = fieldViewProvider;
            _agentThinkingAIController = agentThinkingAIController;
            _agentAIController = agentAIController;
            _userEntitiesModel = userEntitiesModel;
            _botEntitiesModel = botEntitiesModel;
            _fieldModel = fieldModel;
        }

        public async UniTask EnterAsync_DISABLED(CancellationToken ct)
        {
            if (!_agentThinkingAIController.IsInitialized)
                await _agentThinkingAIController.InitializeAsync(ct);

            var diff = AIDifficulty.Insane;
            var cfg = AzDifficultyPresets.Get(diff);
            var mcts = new AzMcts(_agentThinkingAIController, cfg);

            int unityPlayer = 1;
            var azState = AzState.FromUnity(
                _fieldModel.Value,
                _botEntitiesModel.Value,
                _userEntitiesModel.Value,
                unityPlayer
            );

            var (v, row, col) = await mcts.SearchBestMoveAsync(azState, ct, simsPerSlice: 12);

            await _controller.DoMoveAsync(v, new Vector2Int(row, col), ct);
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _controller ??= new UserEntitiesController(_fieldModel.Value, _botEntitiesModel.Value, _fieldViewProvider);


            if (!_agentAIController.IsInitialized)
            {
                await _agentAIController.InitializeAsync(token);
                _agentAIController.StartNewGame();
            }
            _userRoundModelProvider.Value.Model.SetAwaitingTurn(true);
            var (v, row, col) = _agentAIController.ChooseActionVRC(
                _fieldModel.Value,
                _botEntitiesModel.Value,
                _userEntitiesModel.Value,
                _userRoundModelProvider.Value.Model.Owner,
                _userRoundModelProvider.Value.Model.Difficulty);

            await _controller.DoMoveAsync(
                v,
                new Vector2Int(row, col),
                token);
            return Transition.GoTo<ValidateSubstate>();
        }


        public override UniTask Exit(CancellationToken token)
        {
            _userRoundModelProvider.Value.Model.SetAwaitingTurn(false);
            return base.Exit(token);
        }
    }
}