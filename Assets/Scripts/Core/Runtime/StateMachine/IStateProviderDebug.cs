using System;
using UniRx;

namespace Core.StateMachine
{
    public interface IStateProviderDebug: IDisposable
    {
        ReactiveProperty<string> State { get; }
        void ChangeState<T>(T state);
    }
}