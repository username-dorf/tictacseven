using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Server;
using Multiplayer.Lan.Contracts;

namespace Multiplayer.Lan
{
    public class JoinApprovalService : IDisposable
    {
        private readonly HashSet<NetworkConnection> _pending = new();

        public event Action<NetworkConnection, JoinRequest> OnJoinRequested;

        public void Start()
        {
            InstanceFinder.ServerManager.RegisterBroadcast<JoinRequest>(
                OnJoinRequestServer, requireAuthentication: false);
        }

        public void Stop()
        {
            if(InstanceFinder.ServerManager !=null)
                InstanceFinder.ServerManager.UnregisterBroadcast<JoinRequest>(OnJoinRequestServer);
            _pending.Clear();
        }

        public void Dispose()
        {
            Stop();
        }

        private void OnJoinRequestServer(NetworkConnection conn, JoinRequest msg, FishNet.Transporting.Channel ch)
        {
            if (_pending.Contains(conn)) return;
            _pending.Add(conn);

            OnJoinRequested?.Invoke(conn, msg);
        }

        public void Approve(NetworkConnection conn)
        {
            if (!_pending.Remove(conn)) return;
            InstanceFinder.ServerManager.Broadcast(conn, new JoinResponse { Accepted = true, Reason = null });
        }

        public void Reject(NetworkConnection conn, string reason = "Denied by host")
        {
            if (_pending.Remove(conn))
                InstanceFinder.ServerManager.Broadcast(conn, new JoinResponse { Accepted = false, Reason = reason });

            InstanceFinder.ServerManager.Kick(conn, KickReason.ExcessiveData);
        }
    }
}