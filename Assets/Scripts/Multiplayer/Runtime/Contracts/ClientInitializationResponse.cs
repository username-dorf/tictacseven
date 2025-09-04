using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Client send to server init values receive approve
    /// </summary>
    public struct ClientInitializationResponse : IBroadcast
    {
        public bool Accepted;
        public string Reason;
    }
}