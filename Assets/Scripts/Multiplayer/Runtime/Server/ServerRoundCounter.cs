using System;
using UniRx;

namespace Multiplayer.Server
{
    public interface IServerRoundCounter
    {
        ReadOnlyReactiveProperty<int> PassedRounds { get; }
        void IncrementPassedRound();
    }
    public class ServerRoundCounter: IServerRoundCounter, IDisposable
    {
        public ReadOnlyReactiveProperty<int> PassedRounds => _passedRounds.ToReadOnlyReactiveProperty();
        private ReactiveProperty<int> _passedRounds;

        public ServerRoundCounter()
        {
            _passedRounds = new ReactiveProperty<int>(0);
        }
        public void IncrementPassedRound()
        {
            _passedRounds.Value++;
        }

        public void Dispose()
        {
            _passedRounds?.Dispose();
        }
    }
}