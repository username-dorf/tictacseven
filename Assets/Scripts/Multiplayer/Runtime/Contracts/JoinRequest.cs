using Core.User;
using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Client send to server join request
    /// </summary>
    public struct JoinRequest : IBroadcast
    {
        public UserPreferencesDto PreferencesModel;
        public string ClientVersion;
    }
}