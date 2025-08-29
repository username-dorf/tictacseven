using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using UniRx;
using UnityEngine;
using Zenject;

namespace Multiplayer.Lan

{
    public interface ILanHostBroadcaster
    {
        bool IsRunning { get; }
        void Start();
        void Stop();
    }

    public class LanHostBroadcaster : ILanHostBroadcaster, ITickable, IDisposable
    {
        private EventBasedNetListener _listener;
        private NetManager _manager;
        private IDisposable _timerSub;
        public bool IsRunning { get; private set; }

        public void Start()
        {
            if (IsRunning) return;

            _listener = new EventBasedNetListener();
            _manager = new NetManager(_listener)
            {
                UnsyncedEvents = true,
                AutoRecycle = true
            };
            var ok = _manager.Start(0);

            _timerSub = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1.5))
                .Subscribe(_ => Broadcast());

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            _timerSub?.Dispose();
            _timerSub = null;

            _manager?.Stop();
            _manager = null;
            _listener = null;

            IsRunning = false;
        }

        private void Broadcast()
        {
            if (!IsRunning) return;
            var w = new NetDataWriter();
            w.Put(LanConfig.CODE);
            w.Put(SystemInfo.deviceName);

            bool any = false;
            foreach (var ep in GetDirectedBroadcasts(LanConfig.BRODCAST_PORT)) {
                _manager.SendUnconnectedMessage(w, ep);
                any = true;
            }
            if (!any) 
                _manager.SendBroadcast(w, LanConfig.BRODCAST_PORT); // fallback
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            //_manager.SendUnconnectedMessage(w, new IPEndPoint(IPAddress.Loopback, LanConfig.BRODCAST_PORT));
#endif
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
        
        static IEnumerable<IPEndPoint> GetDirectedBroadcasts(int port) {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces()) {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                var ipProps = ni.GetIPProperties();
                foreach (var ua in ipProps.UnicastAddresses) {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork || ua.IPv4Mask == null) continue;
                    uint ip = BitConverter.ToUInt32(ua.Address.GetAddressBytes().Reverse().ToArray(), 0);
                    uint mask = BitConverter.ToUInt32(ua.IPv4Mask.GetAddressBytes().Reverse().ToArray(), 0);
                    uint bcast = ip | ~mask;
                    var bytes = BitConverter.GetBytes(bcast).Reverse().ToArray();
                    yield return new IPEndPoint(new IPAddress(bytes), port);
                }
            }
        }
    }
}