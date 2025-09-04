using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Server send to client request to start turn
    /// </summary>
    public struct ClientTurn : IBroadcast
    {
        public string ActiveClientId;
        
        public long ServerNowTicks;
        public long DeadlineTicks;
        public long ClockFrequency; 
    }
}