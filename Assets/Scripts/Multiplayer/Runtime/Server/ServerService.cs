using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using Multiplayer.Client;
using Multiplayer.Contracts;
using Multiplayer.Server.States;
using UniState;
using UnityEngine;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Server
{
    public class ServerService : IInitializable, IDisposable
    {
        private readonly ServerSubstateScope.Factory _sessionFactory;
        private ServerSubstateScope _currentScope;
        private CancellationTokenSource _stateMachineCts;
        private CancellationTokenSource _heartbeatCts;
        public ServerService(ServerSubstateScope.Factory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public void Initialize()
        {
            InstanceFinder.ServerManager.RegisterBroadcast<ClientLeaveSessionNotice>(OnClientLeaveSessionNotice);
            InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnState;
        }

        public async UniTask LaunchSession(ClientConnection host, ClientConnection opponent, CancellationToken ct)
        {
            StopSession();
            
            _stateMachineCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            _currentScope = _sessionFactory.Create();
            _currentScope.Initialize();
            
            var stateMachine = _currentScope.Resolve<IStateMachine>();
            var payload = new InitializeClientsSubstate.PayloadModel(host, opponent);
            HeartbeatLoop(_heartbeatCts.Token).Forget();
            await stateMachine.Execute<InitializeClientsSubstate, InitializeClientsSubstate.PayloadModel>(payload, _stateMachineCts.Token);
        }
        
        //One of clients request leave via exit button
        private void OnClientLeaveSessionNotice(NetworkConnection from, ClientLeaveSessionNotice msg, Channel arg3)
        {
            InstanceFinder.ServerManager.Broadcast(new TerminateSession
            {
                ClientId = msg.ClientId,
                Reason   = TerminateSessionReason.OPPONENT_LEAVE
            });
            
            StopSession();
        }
        
        //Client crashed or exit app
        private void OnRemoteConnState(NetworkConnection conn, RemoteConnectionStateArgs state)
        {
            if (state.ConnectionState != RemoteConnectionState.Stopped) 
                return;
            
            InstanceFinder.ServerManager.Broadcast(new TerminateSession
            {
                ClientId = "default",
                Reason   = TerminateSessionReason.OPPONENT_LEAVE
            });

            StopSession();
        }
        private async UniTaskVoid HeartbeatLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    InstanceFinder.ServerManager.Broadcast(new ServerHeartbeat());
                    await UniTask.Delay(1000, cancellationToken: ct);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void StopSession()
        {
            _heartbeatCts?.Cancel();
            _heartbeatCts?.Dispose();
            _heartbeatCts = null;

            _stateMachineCts?.Cancel();
            _stateMachineCts?.Dispose();
            _stateMachineCts = null;
            
            _currentScope?.Dispose();
            _currentScope = null;
        }

        public void Dispose()
        {
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientLeaveSessionNotice>(OnClientLeaveSessionNotice);
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnState;
            StopSession();
            Debug.Log("ServerService disposed");
        }
    }
}