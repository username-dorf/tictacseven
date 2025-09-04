using Core.User;
using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Server send to client session init params
    /// </summary>
    public struct ClientInitialization : IBroadcast
    {
        public int Owner;
        
        public UserPreferencesDto Opponent;
        public int OpponentOwner;
    }
}