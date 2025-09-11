using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniState;

namespace Core.StateMachine
{
    public class MultiplayerGameState : AddressablesSceneStateBase
    {
        protected override string SceneAddress => "MultiplayerGame";

        private readonly ManualTransitionTrigger<MultiplayerGameState> _selfTrigger;
        private readonly ManualTransitionTrigger<MenuState> _menuTransitionTrigger;

        public MultiplayerGameState(
            ManualTransitionTrigger<MultiplayerGameState> selfTrigger,
            ManualTransitionTrigger<MenuState> menuTransitionTrigger)
        {
            _selfTrigger = selfTrigger;
            _menuTransitionTrigger = menuTransitionTrigger;
        }

        protected override UniTask OnSceneReady(CancellationToken token)
        {
            _selfTrigger.MarkArrived();
            return UniTask.CompletedTask;
        }

        protected override async UniTask<StateTransitionInfo> ExecuteAfterSceneReady(CancellationToken token)
        {
            try
            {
                return await _menuTransitionTrigger.WaitAndBuildTransitionAsync(Transition, token);
            }
            catch (OperationCanceledException)
            {
                return Transition.GoToExit();
            }
        }

        public override async UniTask Exit(CancellationToken token)
        {
            _selfTrigger.Reset();
            await base.Exit(token);
        }
    }
}