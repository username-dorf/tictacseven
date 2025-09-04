using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Client send to server field state sync approve
    /// </summary>
    public struct ClientFieldSyncResponse : IBroadcast
    {
        public bool Accepted;
        public string Reason;
    }
}