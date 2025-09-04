using System;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.States;
using Multiplayer.Client.States;
using Multiplayer.Contracts;
using Zenject;
using Channel = FishNet.Transporting.Channel;
using RoundResultSubstate = Multiplayer.Client.States.RoundResultSubstate;

namespace Multiplayer.Client
{
    public class ClientSubstateController : IInitializable, IDisposable
    {
        private IStateMachine _stateMachine;
        private IStateMachine _substateMachine;

        public ClientSubstateController(IStateMachine stateMachine, IGameSubstateResolver substateResolver)
        {
            _stateMachine = stateMachine;
            _substateMachine = substateResolver.Resolve<IStateMachine>();

        }
        
        public void Initialize()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<TerminateSession>(OnTerminateRequested);
            InstanceFinder.ClientManager.RegisterBroadcast<ClientTurn>(OnClientTurn);
            InstanceFinder.ClientManager.RegisterBroadcast<RoundResult>(OnRoundResult);

        }

        private void OnRoundResult(RoundResult roundResult, Channel arg2)
        {
            _substateMachine.ChangeStateAsync<RoundResultSubstate,RoundResult>(roundResult, CancellationToken.None)
                .Forget();
        }

        private void OnTerminateRequested(TerminateSession arg1, Channel arg2)
        {
            _stateMachine.ChangeStateAsync<MenuState>(CancellationToken.None)
                .ContinueWith(()=>InstanceFinder.ClientManager.Broadcast(new TerminateSessionResponse()))
                .Forget();
        }
        private void OnClientTurn(ClientTurn request, Channel channel)
        {
            _substateMachine.ChangeStateAsync<TurnSubstate,ClientTurn>(request, CancellationToken.None)
                .Forget();
        }
        public void Dispose()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<TerminateSession>(OnTerminateRequested);
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurn>(OnClientTurn);
            InstanceFinder.ClientManager.UnregisterBroadcast<RoundResult>(OnRoundResult);

        }

    }
}