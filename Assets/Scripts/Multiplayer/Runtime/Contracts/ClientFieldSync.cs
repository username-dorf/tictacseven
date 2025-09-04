using System.Collections.Generic;
using FishNet.Broadcast;
using Game.Entities;
using UnityEngine;

namespace Multiplayer.Contracts
{
    /// <summary>
    /// Server send to all clients current actual field state to sync
    /// </summary>
    public struct ClientFieldSync : IBroadcast
    {
        public Dictionary<Vector2Int, EntityModel.EntityDataSnapshot> FieldModelState;
    }
}