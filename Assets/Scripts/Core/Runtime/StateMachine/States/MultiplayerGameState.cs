using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    
    
    public class MultiplayerGameState: SceneState
    {

        public override async UniTask EnterAsync(CancellationToken ct)
        {
            await LoadSceneAsync("MultiplayerGame", ct);
        }

        public override async UniTask ExitAsync(CancellationToken ct)
        {
            
        }
    }
}