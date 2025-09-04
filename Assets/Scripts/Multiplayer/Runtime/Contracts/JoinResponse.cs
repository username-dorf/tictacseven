using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Server send to client join approve
    /// </summary>
    public struct JoinResponse : IBroadcast
    {
        public bool Accepted;
        public string Reason;
    }
}