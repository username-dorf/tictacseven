using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Multiplayer.Server
{
    public interface IServerAccessor
    {
        ServerService Current { get; }
        bool HasServer { get; }
        void Set(ServerService server);
        void Clear();

        IObservable<ServerService> OnSet { get; }
        IObservable<Unit> OnCleared { get; }

        bool TryGet(out ServerService server);
        UniTask<ServerService> WaitUntilAvailable(CancellationToken ct = default);
    }

    public sealed class ServerAccessor : IServerAccessor, IDisposable
    {
        private readonly ReactiveProperty<ServerService> _current = new ReactiveProperty<ServerService>(null);

        public ServerService Current => _current.Value;
        public bool HasServer => _current.Value != null;

        public void Set(ServerService server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            _current.Value = server;
        }

        public void Clear()
        {
            _current.Value = null;
        }

        public IObservable<ServerService> OnSet =>
            _current.Where(s => s != null);

        public IObservable<Unit> OnCleared =>
            _current.Where(s => s == null).AsUnitObservable();

        public bool TryGet(out ServerService server)
        {
            server = _current.Value;
            return server != null;
        }

        public async UniTask<ServerService> WaitUntilAvailable(CancellationToken ct = default)
        {
            if (_current.Value != null)
                return _current.Value;

            return await _current
                .Where(s => s != null)
                .First()
                .ToUniTask(cancellationToken: ct);
        }

        public void Dispose() => _current?.Dispose();
    }
}