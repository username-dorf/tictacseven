using UnityEngine;
using Zenject;

namespace Multiplayer.Server
{
    public class HostBootstrapper
    {
        private ServerFacade.Factory _factory;
        private IServerAccessor _serverAccessor;

        private ServerFacade _facade;

        public HostBootstrapper(ServerFacade.Factory factory, IServerAccessor serverAccessor)
        {
            _factory = factory;
            _serverAccessor = serverAccessor;
        }

        public void StartHost()
        {
            if (_facade) return;

            _facade = _factory.Create();
            UnityEngine.Object.DontDestroyOnLoad(_facade.gameObject);

            var server = _facade.Context.Container.Resolve<ServerService>();
            _serverAccessor.Set(server);
        }

        public void StopHost()
        {
            _serverAccessor.Clear();
            
            if (_facade == null)
                _facade = GameObject.FindFirstObjectByType<ServerFacade>();
            
            if (_facade)
            {
                UnityEngine.Object.Destroy(_facade.gameObject);
                _facade = null;
            }
        }
    }
}