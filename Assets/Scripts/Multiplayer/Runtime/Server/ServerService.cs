using System;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Multiplayer.Client;
using Multiplayer.Connection;
using Multiplayer.Contracts;
using Multiplayer.Server.States;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Server
{
    public class ServerService : IInitializable, IDisposable
    {
        private readonly ServerSubstateScope.Factory _sessionFactory;

        private ServerSubstateScope _currentScope;

        public ServerService(ServerSubstateScope.Factory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public void Initialize()
        {
            InstanceFinder.ServerManager.RegisterBroadcast<ClientLeaveSessionNotice>(OnClientLeaveSessionNotice);
        }
        public async UniTask LaunchSession(ClientConnection host, ClientConnection opponent, CancellationToken ct)
        {
            StopSession();
            
            _currentScope = _sessionFactory.Create();
            _currentScope.Initialize();
            var stateMachine = _currentScope.Resolve<IStateMachine>();
            var payload = new InitClientsSubstate.Payload(host, opponent);
            await stateMachine.ChangeStateAsync<InitClientsSubstate, InitClientsSubstate.Payload>(payload, ct);
        }
        private void OnClientLeaveSessionNotice(NetworkConnection arg1, ClientLeaveSessionNotice arg2, Channel arg3)
        {
            InstanceFinder.ServerManager.Broadcast(new TerminateSession(){ClientId = arg2.ClientId, Reason = TerminateSessionReason.CLIENT_LEAVE});
        }

        public void StopSession()
        {
            _currentScope?.Dispose();
            _currentScope = null;
        }

        public void Dispose()
        {
            StopSession();
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientLeaveSessionNotice>(OnClientLeaveSessionNotice);
        }
    }
}