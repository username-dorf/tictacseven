using System;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Multiplayer.Contracts;
using Multiplayer.Server;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Connection
{
    public class SessionController : IInitializable, IDisposable
    {
        private readonly IHostBroadcaster _hostBroadcaster;
        private readonly IClientScanner _clientScanner;
        private readonly ConnectionController _launcher;
        private readonly JoinApprovalService _joinApprovalService;
        private readonly HostBootstrapper _hostBootstrapper;
        private Action<UserPreferencesDto, string> _clientScannerOnOnHostDiscovered;

        public SessionController(
            IHostBroadcaster host,
            IClientScanner scanner,
            ConnectionController launcher,
            JoinApprovalService joinApprovalService,
            HostBootstrapper hostBootstrapper)
        {
            _hostBootstrapper = hostBootstrapper;
            _joinApprovalService = joinApprovalService;
            _hostBroadcaster = host;
            _clientScanner = scanner;
            _launcher = launcher;
        }
        
        public void Initialize()
        {
            InstanceFinder.ServerManager.RegisterBroadcast<TerminateSessionResponse>(OnClientApproveTerminate);
        }

        public void CreateSession()
        {
            _hostBootstrapper.StartHost();
            _clientScanner.Stop();
            _hostBroadcaster.Start();
            _launcher.StartHost();
            _joinApprovalService.Start();
        }

        public void StartDiscovery(Action<UserPreferencesDto,string> onFound)
        {
            _hostBroadcaster.Stop();
            _clientScanner.Stop();
            
            if(_clientScannerOnOnHostDiscovered!=null)
                _clientScanner.OnHostDiscovered -= _clientScannerOnOnHostDiscovered;
            
            _clientScannerOnOnHostDiscovered = onFound;
            _clientScanner.OnHostDiscovered += _clientScannerOnOnHostDiscovered;
            
            _clientScanner.Start();
        }

        public void StopDiscovery()
        {
            if (_clientScannerOnOnHostDiscovered != null)
            {
                _clientScanner.OnHostDiscovered -= _clientScannerOnOnHostDiscovered;
                _clientScannerOnOnHostDiscovered = null;
            }

            _hostBroadcaster.Stop();
            _clientScanner.Stop();
        }

        public void ConnectTo(string ip)
        {
            if (_clientScannerOnOnHostDiscovered != null)
            {
                _clientScanner.OnHostDiscovered -= _clientScannerOnOnHostDiscovered;
                _clientScannerOnOnHostDiscovered = null;
            }
            
            _clientScanner.Stop();
            _launcher.ConnectToHost(ip);
        }
        public void StopConnecting()
        {
            _clientScanner.Stop();
            _launcher.CloseConnection();
        }

        public void CancelHosting()
        {
            _hostBootstrapper.StopHost();
            _joinApprovalService.Stop();
            _launcher.StopHost();
            _hostBroadcaster.Stop();
        }

        private async void OnClientApproveTerminate(NetworkConnection arg1, TerminateSessionResponse arg2, Channel arg3)
        {
            await UniTask.WaitForEndOfFrame();
            CancelHosting();
        }

        public void Dispose()
        {
            InstanceFinder.ServerManager?.UnregisterBroadcast<TerminateSessionResponse>(OnClientApproveTerminate);
        }
    }
}