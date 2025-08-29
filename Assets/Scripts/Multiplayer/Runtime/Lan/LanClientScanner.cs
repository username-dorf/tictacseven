using System;
using System.Collections.Generic;
using System.Net;
using LiteNetLib;
using Zenject;

namespace Multiplayer.Lan
{
    public interface ILanClientScanner
    {
        bool IsRunning { get; }
        event Action<string, string> OnHostDiscovered;
        void Start();
        void Stop();
    }

    public class LanClientScanner : ILanClientScanner, ITickable, IDisposable
    {
        private EventBasedNetListener _listener;
        private NetManager _manager;
        private readonly HashSet<string> _seen = new();
        public bool IsRunning { get; private set; }
        public event Action<string, string> OnHostDiscovered;

        public void Start()
        {
            if (IsRunning) return;

            _listener = new EventBasedNetListener();
            _listener.NetworkReceiveUnconnectedEvent += OnReceive;

            _manager = new NetManager(_listener)
            {
                UnconnectedMessagesEnabled = true,
                BroadcastReceiveEnabled = true,
                UnsyncedEvents = true,
                AutoRecycle = true,
                IPv6Enabled = false
            };
            var ok =_manager.Start(LanConfig.BRODCAST_PORT);

            _seen.Clear();
            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            _listener.NetworkReceiveUnconnectedEvent -= OnReceive;
            _manager?.Stop();
            _manager = null;
            _listener = null;
            _seen.Clear();

            IsRunning = false;
        }

        private void OnReceive(IPEndPoint point, NetPacketReader reader, UnconnectedMessageType type)
        {
            if (type != UnconnectedMessageType.Broadcast && type != UnconnectedMessageType.BasicMessage)
                return;

            string header = reader.GetString();
            if (header != LanConfig.CODE) return;

            string hostName = reader.GetString();
            string ip = point.Address.ToString();

            if (_seen.Add(ip))
                OnHostDiscovered?.Invoke(hostName, ip);
        }

        public void Tick()
        {
            if (IsRunning)
                _manager?.PollEvents();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}