using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniState;

namespace Core.StateMachine
{
    public interface IGameBootstrapAsync : IDisposable
    {
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }

    public class GameState : AddressablesSceneStateBase
    {
        protected override string SceneAddress => "Game";

        private IGameBootstrapAsync _gameBootstrap;

        private readonly ManualTransitionTrigger<GameState> _selfTrigger;

        private readonly ManualTransitionTrigger<MenuState> _menuTransitionTrigger;

        public GameState(
            ManualTransitionTrigger<GameState> selfTrigger,
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