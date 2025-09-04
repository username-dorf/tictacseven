using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Core.User;
using LiteNetLib;
using LiteNetLib.Utils;
using UniRx;
using UnityEngine;
using Zenject;

namespace Multiplayer.Connection

{
    public interface IHostBroadcaster
    {
        bool IsRunning { get; }
        void Start();
        void Stop();
    }

    public class HostBroadcaster : IHostBroadcaster, ITickable, IDisposable
    {
        private EventBasedNetListener _listener;
        private NetManager _manager;
        private IDisposable _timerSub;
        
        private IUserPreferencesProvider _userPreferencesProvider;
        public bool IsRunning { get; private set; }

        public HostBroadcaster(IUserPreferencesProvider userPreferencesProvider)
        {
            _userPreferencesProvider = userPreferencesProvider;
        }

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
            var writer = new NetDataWriter();
            writer.Put(ConnectionConfig.CODE);
            writer.Put(SystemInfo.deviceName);
            writer.Put(UserPreferencesDto.Create(_userPreferencesProvider.Current));

            bool any = false;
            foreach (var ep in GetDirectedBroadcasts(ConnectionConfig.BRODCAST_PORT)) {
                _manager.SendUnconnectedMessage(writer, ep);
                any = true;
            }
            if (!any) 
                _manager.SendBroadcast(writer, ConnectionConfig.BRODCAST_PORT); // fallback
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