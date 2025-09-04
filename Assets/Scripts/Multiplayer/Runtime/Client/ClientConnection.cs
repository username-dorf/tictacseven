using Core.User;
using FishNet.Connection;

namespace Multiplayer.Client
{
    public class ClientConnection
    {
        public UserPreferencesDto Preferences { get; }
        public NetworkConnection Connection { get; }

        public ClientConnection(NetworkConnection connection, UserPreferencesDto preferences)
        {
            Preferences = preferences;
            Connection = connection;
        }
    }
}