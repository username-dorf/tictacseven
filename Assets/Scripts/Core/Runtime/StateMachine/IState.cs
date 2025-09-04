using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Core.StateMachine
{
    public interface IState
    {
        UniTask EnterAsync(CancellationToken cancellationToken);
        UniTask ExitAsync(CancellationToken cancellationToken);
    }
}