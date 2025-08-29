using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public class MenuState : SceneState
    {
        public override async UniTask EnterAsync(CancellationToken ct)
        {
            await LoadSceneAsync("Menu", ct);
        }

        public override async UniTask ExitAsync(CancellationToken cancellationToken)
        {
            
        }
        
    }
}