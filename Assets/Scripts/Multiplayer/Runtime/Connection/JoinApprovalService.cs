using System;
using System.Collections.Generic;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Server;
using Multiplayer.Contracts;
using UniRx;
using UnityEngine;

namespace Multiplayer.Connection
{
    public interface IOpponentConnectionListener
    {
        ReactiveCommand<(NetworkConnection conn, UserPreferencesDto prefs)> OnConnectionApproved { get; }
    }
    public class JoinApprovalService :IOpponentConnectionListener, IDisposable
    {
        private readonly HashSet<NetworkConnection> _pending = new();
 
        public event Action<NetworkConnection, JoinRequest> OnJoinRequested;
        public ReactiveCommand<(NetworkConnection conn, UserPreferencesDto prefs)> OnConnectionApproved { get; } = new();

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
            OnConnectionApproved?.Dispose();
        }

        private void OnJoinRequestServer(NetworkConnection conn, JoinRequest msg, FishNet.Transporting.Channel ch)
        {
            if (_pending.Contains(conn)) return;
            _pending.Add(conn);

            OnJoinRequested?.Invoke(conn, msg);
            Debug.Log("Join request received from " + conn.ClientId);
        }

        public void Approve(NetworkConnection conn, JoinRequest request)
        {
            if (!_pending.Remove(conn)) 
                return;
            InstanceFinder.ServerManager.Broadcast(conn, new JoinResponse { Accepted = true, Reason = null });
            OnConnectionApproved?.Execute((conn,request.PreferencesModel));
        }

        public async void Reject(NetworkConnection conn, string reason = "Denied by host")
        {
            if (_pending.Remove(conn))
                InstanceFinder.ServerManager.Broadcast(conn, new JoinResponse { Accepted = false, Reason = reason });

            await UniTask.Delay(TimeSpan.FromSeconds(1f)); //some time to client to receive reject
            InstanceFinder.ServerManager.Kick(conn, KickReason.ExcessiveData);
        }
    }
}