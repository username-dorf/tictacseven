using System;

namespace Multiplayer.Lan
{
    public class LanController
    {
        private readonly ILanHostBroadcaster _hostBroadcaster;
        private readonly ILanClientScanner _clientScanner;
        private readonly ConnectionController _launcher;
        private readonly JoinApprovalService _joinApprovalService;
        private readonly JoinResponseListener _joinResponseListener;
        private Action<string, string> _clientScannerOnOnHostDiscovered;

        public LanController(
            ILanHostBroadcaster host,
            ILanClientScanner scanner,
            ConnectionController launcher,
            JoinApprovalService joinApprovalService,
            JoinResponseListener joinResponseListener)
        {
            _joinResponseListener = joinResponseListener;
            _joinApprovalService = joinApprovalService;
            _hostBroadcaster = host;
            _clientScanner = scanner;
            _launcher = launcher;
        }

        public void CreateSession()
        {
            _clientScanner.Stop();
            _hostBroadcaster.Start();
            _launcher.StartHost();
            _joinApprovalService.Start();
        }

        public void StartDiscovery(Action<string,string> onFound)
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
            _joinResponseListener.Start();
            _launcher.ConnectToHost(ip);
        }

        public void CancelHosting()
        {
            _joinApprovalService.Stop();
            _launcher.StopHost();
            _hostBroadcaster.Stop();
        }
    }
}