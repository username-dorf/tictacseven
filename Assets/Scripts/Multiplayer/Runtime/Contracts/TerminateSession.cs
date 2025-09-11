using FishNet.Broadcast;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Server send to client terminate session
    /// </summary>
    public struct TerminateSession : IBroadcast
    {
        public string ClientId;
        public string Reason;
    }

    public class TerminateSessionReason
    {
        public const string OPPONENT_LEAVE = "opponent leave game";
        public const string HOST_DISCONNECT = "host disconnected";
    }
}