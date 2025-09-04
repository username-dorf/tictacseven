using System;
using FishNet;
using FishNet.Transporting;
using Multiplayer.Contracts;
using UniRx;

namespace Multiplayer.Connection
{
    public class JoinResponseListener
    {
        private readonly Subject<JoinResponse> _subject;
        public IObservable<JoinResponse> OnResponse => _subject;

        private bool _started;

        public JoinResponseListener()
        {
            _subject = new Subject<JoinResponse>();
        }

        public void Start()
        {
            if (_started) return;
            _started = true;
            InstanceFinder.ClientManager.RegisterBroadcast<JoinResponse>(OnJoinResponse);
        }

        public void Stop()
        {
            if (!_started) return;
            _started = false;
            InstanceFinder.ClientManager.UnregisterBroadcast<JoinResponse>(OnJoinResponse);
        }

        private void OnJoinResponse(JoinResponse msg, Channel _)
        {
            _subject.OnNext(msg);
        }
    }

}