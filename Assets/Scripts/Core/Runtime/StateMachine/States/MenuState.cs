using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public class MenuState : SceneState
    {
        public override async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            await LoadSceneAsync("Menu", cancellationToken);
        }

        public override async UniTask ExitAsync(CancellationToken cancellationToken)
        {
            
        }
        
    }
}