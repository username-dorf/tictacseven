using System;
using Zenject;

namespace Multiplayer.Server
{
    public interface IServerSubstateResolver
    {
        public T Resolve<T>();
    }
    public class ServerSubstateScope : IServerSubstateResolver
    {
        private DiContainer _sub;

        public ServerSubstateScope(DiContainer sub)
        {
            _sub = sub;
        }

        public void Initialize()
        {
            _sub.Resolve<InitializableManager>().Initialize();
        }
        public T Resolve<T>()
        {
            return _sub.Resolve<T>();
        }

        public void Dispose()
        {
            _sub.TryResolve<DisposableManager>()?.Dispose();
            _sub = null;
        }

        public class Factory : PlaceholderFactory<ServerSubstateScope> { }
    }
}