using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Client send to server notice about leaving session
    /// </summary>
    public struct ClientLeaveSessionNotice : IBroadcast
    {
        public string ClientId;
    }
}