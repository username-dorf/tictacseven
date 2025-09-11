using System.Threading;
using Core.Audio;
using Cysharp.Threading.Tasks;
using UniState;
using UnityEngine.SceneManagement;

namespace Core.StateMachine
{
    public sealed class MenuState : AddressablesSceneStateBase
    {
        protected override string SceneAddress => "Menu";
        protected override LoadSceneMode LoadMode => LoadSceneMode.Single;

        private readonly ManualTransitionTrigger<MenuState> _selfTrigger;

        private readonly ManualTransitionTrigger<GameState> _goGame;
        private readonly ManualTransitionTrigger<MultiplayerGameState> _goMultiplayer;
        private readonly SceneMusicBootstrap _sceneMusicBootstrap;

        public MenuState(
            SceneMusicBootstrap sceneMusicBootstrap,
            ManualTransitionTrigger<MenuState> selfTrigger,
            ManualTransitionTrigger<GameState> goGame,
            ManualTransitionTrigger<MultiplayerGameState> goMultiplayer)
        {
            _sceneMusicBootstrap = sceneMusicBootstrap;
            _selfTrigger = selfTrigger;
            _goGame = goGame;
            _goMultiplayer = goMultiplayer;
        }

        protected override UniTask OnSceneReady(CancellationToken token)
        {
            _sceneMusicBootstrap.Initialize();
            
            _selfTrigger.MarkArrived();
            return UniTask.CompletedTask;
        }

        protected override async UniTask<StateTransitionInfo> ExecuteAfterSceneReady(CancellationToken token)
        {
            var gameTask = _goGame.WaitAndBuildTransitionAsync(Transition, token);
            var multiTask = _goMultiplayer.WaitAndBuildTransitionAsync(Transition, token);
            var cancelTask = UniTask.Create(async () =>
            {
                await UniTask.WaitUntilCanceled(token);
                return Transition.GoToExit();
            });

            var (i, rGame, rMulti, rCancel) = await UniTask.WhenAny(gameTask, multiTask, cancelTask);

            return i switch
            {
                0 => rGame,
                1 => rMulti,
                2 => rCancel,
                _ => Transition.GoToExit()
            };
        }

        public override async UniTask Exit(CancellationToken token)
        {
            _selfTrigger.Reset();
            await base.Exit(token);
        }
    }
}