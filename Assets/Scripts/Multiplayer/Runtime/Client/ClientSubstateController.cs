using System;
using System.Threading;
using Core.StateMachine;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Transporting;
using Multiplayer.Contracts;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Client
{
    public interface ISessionExitClient
    {
        UniTask LeaveByUserAsync();
    }
    public class ClientSubstateController : IInitializable, IDisposable, ISessionExitClient
    {
        private IWindowsController _windowsController;
        private IUserPreferencesProvider _userPreferencesProvider;
        private ManualTransitionTrigger<MenuState> _menuTransitionTrigger;
        
        private int  _notifiedOnce;
        private bool _selfLeaving;
        private CancellationTokenSource _subCts;


        public ClientSubstateController(
            ManualTransitionTrigger<MenuState> menuTransitionTrigger,
            IWindowsController windowsController,
            IUserPreferencesProvider userPreferencesProvider)
        {
            _menuTransitionTrigger = menuTransitionTrigger;
            _userPreferencesProvider = userPreferencesProvider;
            _windowsController = windowsController;
        }
        
        public void Initialize()
        {
            _subCts = new CancellationTokenSource();
            _notifiedOnce = 0;
            _selfLeaving = false;
            
            InstanceFinder.ClientManager.RegisterBroadcast<TerminateSession>(OnTerminateRequested);
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionStateChanged;
            InstanceFinder.ClientManager.OnClientTimeOut += OnClientTimedOut;

        }
        public async UniTask LeaveByUserAsync()
        {
            _selfLeaving = true;

            await NotifyAndReturnToMenuOnceAsync(reason: null, sendAck: false);
            var clientId = _userPreferencesProvider.Current.User.Id;
            InstanceFinder.ClientManager.Broadcast(new ClientLeaveSessionNotice { ClientId = clientId });
            InstanceFinder.ClientManager.StopConnection();
        }

        private void OnTerminateRequested(TerminateSession msg, Channel _)
        {
            if (msg.ClientId == _userPreferencesProvider.Current.User.Id) 
                return;

            UniTask.Void(async () =>
            {
                await NotifyAndReturnToMenuOnceAsync(
                    reason: string.IsNullOrEmpty(msg.Reason) ? null : msg.Reason,
                    sendAck: true
                );
            });
        }
        private void OnClientConnectionStateChanged(ClientConnectionStateArgs clientConnectionStateArgs)
        {
            if (InstanceFinder.IsHostStarted)
                return;
            
            if (_selfLeaving)
                return;

            if (clientConnectionStateArgs.ConnectionState == LocalConnectionState.Stopped)
            {
                UniTask.Void(async () =>
                {
                    await NotifyAndReturnToMenuOnceAsync(TerminateSessionReason.HOST_DISCONNECT, sendAck: false);
                });
            }
        }
        
        private async UniTask NotifyAndReturnToMenuOnceAsync(string reason, bool sendAck)
        {
            if (Interlocked.CompareExchange(ref _notifiedOnce, 1, 0) != 0)
                return;

            await UniTask.SwitchToMainThread();

            _subCts?.Cancel();

            _menuTransitionTrigger.Continue();
            
            await _menuTransitionTrigger.WhenArrivedAsync(CancellationToken.None);

            if (sendAck)
                InstanceFinder.ClientManager.Broadcast(new TerminateSessionResponse());
            
            if (!string.IsNullOrEmpty(reason))
            {
                var payload = new UIWindowModal.Payload(reason, null);
                await _windowsController.OpenAsync<UIWindowModal, UIWindowModal.Payload>(payload, CancellationToken.None);
            }

            _subCts?.Dispose();
            _subCts = new CancellationTokenSource();
        }
        private void OnClientTimedOut()
        {
            UniTask.Void(async () =>
            {
                await NotifyAndReturnToMenuOnceAsync(TerminateSessionReason.HOST_DISCONNECT, sendAck:false);
            });
        }
        public void Dispose()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<TerminateSession>(OnTerminateRequested);
            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionStateChanged;
            InstanceFinder.ClientManager.OnClientTimeOut -= OnClientTimedOut;

            _subCts?.Cancel();
            _subCts?.Dispose();
        }
    }
}