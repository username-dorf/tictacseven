using FishNet.Broadcast;

namespace Multiplayer.Lan.Contracts
{
    public struct JoinRequest : IBroadcast
    {
        public string PlayerName;
        public string ClientVersion;
    }
}