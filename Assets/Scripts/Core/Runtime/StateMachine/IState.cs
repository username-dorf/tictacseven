using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Core.StateMachine
{
    public interface IState: IDisposable
    {
        UniTask EnterAsync(CancellationToken ct);
        UniTask ExitAsync(CancellationToken cancellationToken);
    }
}