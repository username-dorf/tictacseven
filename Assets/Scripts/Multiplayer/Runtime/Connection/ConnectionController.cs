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
            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
            _clientSessionBootstrap.Launch();
        }

        public void ConnectToHost(string ip)
        {
            _networkManager.TransportManager.Transport.SetClientAddress(ip);

            void Handler(ClientConnectionStateArgs args)
            {
                if (args.ConnectionState == LocalConnectionState.Started)
                {
                    var model = UserPreferencesDto.Create(_userPreferences.Current);
                    var req = new JoinRequest {
                        PreferencesModel = model,
                        ClientVersion = Application.version
                    };
                    _networkManager.ClientManager.Broadcast(req);
                    _networkManager.ClientManager.OnClientConnectionState -= Handler;
                }
            }

            _networkManager.ClientManager.OnClientConnectionState += Handler;
            _networkManager.ClientManager.StartConnection();
            _clientSessionBootstrap.Launch();
        }

        public void CloseConnection()
        {
            _networkManager.ClientManager.StopConnection();
        }

        public void StopHost()
        {
            _networkManager.ClientManager.StopConnection();
            _networkManager.ServerManager.StopConnection(true);
        }
    }
}