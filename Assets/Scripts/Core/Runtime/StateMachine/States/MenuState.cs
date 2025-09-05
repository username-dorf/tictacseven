using System.Threading;
using Core.Audio;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public class MenuState : SceneState
    {
        private SceneMusicBootstrap _sceneMusicBootstrap;

        public MenuState(SceneMusicBootstrap sceneMusicBootstrap)
        {
            _sceneMusicBootstrap = sceneMusicBootstrap;
        }
        public override async UniTask EnterAsync(CancellationToken ct)
        {
            await LoadSceneAsync("Menu", ct);
            _sceneMusicBootstrap.Initialize();
        }

        public override async UniTask ExitAsync(CancellationToken ct)
        {
            
        }
        
    }
}