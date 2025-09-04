using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Client send to server after round result window
    /// </summary>
    public struct RoundResultResponse: IBroadcast
    {
        public bool Accepted;
    }
}