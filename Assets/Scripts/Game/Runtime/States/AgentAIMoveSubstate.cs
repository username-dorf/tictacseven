using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Game.Field;
using Game.User;
using UnityEngine;
using Zenject;

namespace Game.States
{
    public class AgentAIMoveSubstate : GameSubstate
    {
        public const string AGENT_MODEL_ID = "AgentAIModel";
        private FieldModel _fieldModel;
        private UserEntitiesModel _botEntitiesModel;
        private UserEntitiesController _controller;
        private UserEntitiesModel _userEntitiesModel;
        private AgentAIController _agentAIController;
        private AgentThinkingAIController _agentThinkingAIController;

        public AgentAIMoveSubstate(
            FieldModel fieldModel,
            FieldViewProvider fieldViewProvider,
            [Inject(Id = AGENT_MODEL_ID)] UserEntitiesModel botEntitiesModel,
            [Inject(Id = UserMoveSubstate.AGENT_MODEL_ID)]
            UserEntitiesModel userEntitiesModel,
            AgentAIController agentAIController,
            AgentThinkingAIController agentThinkingAIController,
            IGameSubstateResolver gameSubstateResolver) : base(gameSubstateResolver)
        {
            _agentThinkingAIController = agentThinkingAIController;
            _agentAIController = agentAIController;
            _userEntitiesModel = userEntitiesModel;
            _botEntitiesModel = botEntitiesModel;
            _fieldModel = fieldModel;
            _controller = new UserEntitiesController(_fieldModel, _botEntitiesModel, fieldViewProvider);
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
                _fieldModel,
                _botEntitiesModel,
                _userEntitiesModel,
                unityPlayer
            );

            var (v, row, col) = await mcts.SearchBestMoveAsync(azState, ct, simsPerSlice: 12);

            await _controller.DoMoveAsync(v, new Vector2Int(row, col), ct);
        }

        public override async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            Debug.Log("AgentAIMoveSubstate: EnterAsync");

            if (!_agentAIController.IsInitialized)
            {
                await _agentAIController.InitializeAsync(cancellationToken);
                _agentAIController.StartNewGame();
            }

            int unityPlayer = 1;
            var (v, row, col) = _agentAIController.ChooseActionVRC(
                _fieldModel,
                _botEntitiesModel,
                _userEntitiesModel,
                unityPlayer, PolicyDifficulty.Normal);

            await _controller.DoMoveAsync(
                v,
                new Vector2Int(row, col),
                cancellationToken);
            await SubstateMachine.ChangeStateAsync<ValidateSubstate>();
        }


        public override UniTask ExitAsync(CancellationToken _)
        {
            Debug.Log("AgentAIMoveSubstate: ExitAsync");

            return UniTask.CompletedTask;
        }
    }
}