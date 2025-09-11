using Core.User;
using FishNet.Managing;
using FishNet.Transporting;
using Multiplayer.Client;
using Multiplayer.Contracts;
using UnityEngine;

namespace Multiplayer.Connection
{
    public class ConnectionController
    {
        private NetworkManager _networkManager;
        private IUserPreferencesProvider _userPreferences;
        private ClientSessionBootstrap _clientSessionBootstrap;

        public ConnectionController(
            NetworkManager networkManager,
            IUserPreferencesProvider userPreferences,
            ClientSessionBootstrap clientSessionBootstrap)
        {
            _clientSessionBootstrap = clientSessionBootstrap;
            _userPreferences = userPreferences;
            _networkManager = networkManager;
        }

        public void StartHost()
        {
            _networkManager.ClientManager.OnClientConnectionState -= Handler;

            if (_networkManager.ClientManager.Started)
                _networkManager.ClientManager.StopConnection();

            _networkManager.ServerManager.StartConnection();

            var tr = _networkManager.TransportManager.Transport;
            TrySetClientAddress(tr, ConnectionConfig.LOCAL_CLIENT);
            TrySetClientPort(tr, tr.GetPort());

            _networkManager.ClientManager.StartConnection();

            _clientSessionBootstrap.Launch();
        }
        private static void TrySetClientAddress(Transport tr, string address)
        {
            try { tr.SetClientAddress(address); } catch { }
        }
        private static void TrySetClientPort(Transport tr, ushort port)
        {
            try { tr.SetPort(port); } catch { }
        }

        public void ConnectToHost(string ip)
        {
            _networkManager.ClientManager.OnClientConnectionState -= Handler;

            _networkManager.TransportManager.Transport.SetClientAddress(ip);
            _networkManager.ClientManager.OnClientConnectionState += Handler;
            _networkManager.ClientManager.StartConnection();
            _clientSessionBootstrap.Launch();
        }

        private void Handler(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                var model = UserPreferencesDto.Create(_userPreferences.Current);
                var req = new JoinRequest { PreferencesModel = model, ClientVersion = Application.version };
                _networkManager.ClientManager.Broadcast(req);
                _networkManager.ClientManager.OnClientConnectionState -= Handler;
            }
            else if (args.ConnectionState == LocalConnectionState.Stopped || 
                     args.ConnectionState == LocalConnectionState.Stopping)
            {
                _networkManager.ClientManager.OnClientConnectionState -= Handler;
            }
        }
        
        public void CloseConnection()
        {
            _networkManager.ClientManager.OnClientConnectionState -= Handler;
            _networkManager.ClientManager.StopConnection();
        }

        public void StopHost()
        {
            _networkManager.ClientManager.StopConnection();
            _networkManager.ServerManager.StopConnection(true);
        }
    }
}