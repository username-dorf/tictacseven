using FishNet.Broadcast;
using Game.Entities;
using UnityEngine;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Client send to server result of turn
    /// </summary>
    public struct ClientTurnResponse: IBroadcast
    {
        public string ClientId;
        public int Merit;
        public Vector2Int Coordinates;
        public EntityModel EntityModel;
    }
}