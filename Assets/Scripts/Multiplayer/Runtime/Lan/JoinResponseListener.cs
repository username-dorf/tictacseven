using System;
using FishNet;
using FishNet.Transporting;
using Multiplayer.Lan.Contracts;
using UnityEngine;

namespace Multiplayer.Lan
{
    public class JoinResponseListener
    {
        public void Start()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<JoinResponse>(OnJoinResponse);
        }

        private void OnJoinResponse(JoinResponse response, Channel arg2)
        {
            if (!response.Accepted)
            {
                Debug.Log($"Join rejected: {response.Reason}");
            }
            else
            {
                Debug.Log("Join approved!");
            }
            Stop();
        }

        private void Stop()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<JoinResponse>(OnJoinResponse);
        }
    }
}