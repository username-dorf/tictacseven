using System.Collections.Generic;
using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Server send to client round result (winner)
    /// </summary>
    public struct RoundResult: IBroadcast
    {
        public List<string> WinnerIds;
    }
}