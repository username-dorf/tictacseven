using System;
using System.Threading;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using Multiplayer.Client;
using Multiplayer.Connection;
using UniRx;
using UnityEngine;
using Zenject;

namespace Multiplayer.Server
{
    public class ServerSessionBootstrap : IInitializable
    {
        private IOpponentConnectionListener _opponentConnectionListener;
        private IHostConnectionProvider _hostConnectionProvider;
        private IUserPreferencesProvider _preferencesProvider;

        private CompositeDisposable _disposable;
        private IServerAccessor _serverAccessor;

        public ServerSessionBootstrap(
            IServerAccessor serverAccessor,
            IOpponentConnectionListener opponentConnectionListener,
            IHostConnectionProvider hostConnectionProvider,
            IUserPreferencesProvider preferencesProvider)
        {
            _serverAccessor = serverAccessor;
            _preferencesProvider = preferencesProvider;
            _hostConnectionProvider = hostConnectionProvider;
            _opponentConnectionListener = opponentConnectionListener;
            _disposable = new CompositeDisposable();
        }

        public void Initialize()
        {
            _opponentConnectionListener.OnConnectionApproved
                .Subscribe(LaunchSession)
                .AddTo(_disposable);
        }

        private void LaunchSession((NetworkConnection opponentConnection, UserPreferencesDto opponentPreferences) data)
        {
            var hasHostConnection = _hostConnectionProvider.TryGetConnection(out var hostConnection);
            if (!hasHostConnection)
                throw new Exception("Can't resolve host network connection");
            var hostPreferences = UserPreferencesDto.Create(_preferencesProvider.Current);
            var hostClientConnection = new ClientConnection(hostConnection, hostPreferences);
            var opponentClientConnection = new ClientConnection(data.opponentConnection, data.opponentPreferences);

            _serverAccessor.Current.LaunchSession(hostClientConnection, opponentClientConnection,CancellationToken.None)
                .Forget();
            
            Debug.Log($"Game launched for {data.opponentConnection.ClientId} and {hostConnection.ClientId}");
        }
    }
}