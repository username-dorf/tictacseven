using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Server send to client terminate turn due to timeout
    /// </summary>
    public struct ClientTurnTimeout: IBroadcast
    {
        public string OffenderClientId;
    }
}