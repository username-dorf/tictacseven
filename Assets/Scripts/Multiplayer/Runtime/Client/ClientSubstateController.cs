using System;
using System.Threading;
using Core.StateMachine;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Core.User;
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
        private IWindowsController _windowsController;
        private IUserPreferencesProvider _userPreferencesProvider;

        public ClientSubstateController(
            IStateMachine stateMachine,
            IWindowsController windowsController,
            IGameSubstateResolver substateResolver,
            IUserPreferencesProvider userPreferencesProvider)
        {
            _userPreferencesProvider = userPreferencesProvider;
            _windowsController = windowsController;
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
            var ct = CancellationToken.None;
            _stateMachine.ChangeStateAsync<MenuState>(ct)
                .ContinueWith(()=>InstanceFinder.ClientManager.Broadcast(new TerminateSessionResponse()))
                .ContinueWith(()=>OnTerminateSessionApproved(arg1,ct))
                .Forget();
        }
        private void OnClientTurn(ClientTurn request, Channel channel)
        {
            _substateMachine.ChangeStateAsync<TurnSubstate,ClientTurn>(request, CancellationToken.None)
                .Forget();
        }

        private async UniTask OnTerminateSessionApproved(TerminateSession request, CancellationToken ct)
        {
            if(string.IsNullOrEmpty(request.Reason))
                return;
            if(request.ClientId == _userPreferencesProvider.Current.User.Id)
                return;
            var payload = new UIWindowModal.Payload(request.Reason, null);
            _windowsController.OpenAsync<UIWindowModal, UIWindowModal.Payload>(payload, ct);
        }
        public void Dispose()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<TerminateSession>(OnTerminateRequested);
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurn>(OnClientTurn);
            InstanceFinder.ClientManager.UnregisterBroadcast<RoundResult>(OnRoundResult);

        }

    }
}