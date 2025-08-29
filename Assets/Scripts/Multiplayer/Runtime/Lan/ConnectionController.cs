using Core.User;
using FishNet.Managing;
using FishNet.Transporting;
using Multiplayer.Lan.Contracts;
using UnityEngine;

namespace Multiplayer.Lan
{
    public class ConnectionController
    {
        private NetworkManager _networkManager;
        private IUserPreferencesProvider _userPreferences;

        public ConnectionController(NetworkManager networkManager, IUserPreferencesProvider userPreferences)
        {
            _userPreferences = userPreferences;
            _networkManager = networkManager;
        }

        public void StartHost()
        {
            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
        }

        public void ConnectToHost(string ip)
        {
            _networkManager.TransportManager.Transport.SetClientAddress(ip);

            void Handler(ClientConnectionStateArgs args)
            {
                if (args.ConnectionState == LocalConnectionState.Started)
                {
                    var req = new JoinRequest {
                        PlayerName = _userPreferences.Current.User.Nickname.Value ?? UnityEngine.SystemInfo.deviceName,
                        ClientVersion = Application.version
                    };
                    _networkManager.ClientManager.Broadcast(req);
                    _networkManager.ClientManager.OnClientConnectionState -= Handler;
                }
            }

            _networkManager.ClientManager.OnClientConnectionState += Handler;
            _networkManager.ClientManager.StartConnection();
        }

        public void StopHost()
        {
            _networkManager.ClientManager.StopConnection();
            _networkManager.ServerManager.StopConnection(true);
        }
    }
}