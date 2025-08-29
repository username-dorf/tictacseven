using FishNet.Broadcast;

namespace Multiplayer.Lan.Contracts
{
    public struct JoinResponse : IBroadcast
    {
        public bool Accepted;
        public string Reason;
    }
}