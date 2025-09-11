using System;
using System.Threading;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet;
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
                .Subscribe(data=>LaunchSession(data).Forget())
                .AddTo(_disposable);
        }

        private async UniTask LaunchSession((NetworkConnection opponentConnection, UserPreferencesDto opponentPreferences) data)
        {
            NetworkConnection hostConnection = null;
            try
            {
                if (!_hostConnectionProvider.TryGetConnection(out hostConnection))
                    hostConnection = await WaitHostConnAsync(3000);
            }
            catch (OperationCanceledException)
            {
                Debug.LogError(
                    $"[Server] Can't resolve host network connection (timeout). Clients={InstanceFinder.ServerManager.Clients.Count}");
                foreach (var kv in InstanceFinder.ServerManager.Clients)
                    Debug.Log($"[Server] Client {kv.Key}: IsLocalClient={kv.Value.IsLocalClient}");
                return;
            }

            var hostPreferences = UserPreferencesDto.Create(_preferencesProvider.Current);
            var hostClientConnection = new ClientConnection(hostConnection, hostPreferences);
            var opponentClientConnection = new ClientConnection(data.opponentConnection, data.opponentPreferences);

            _serverAccessor.Current
                .LaunchSession(hostClientConnection, opponentClientConnection, CancellationToken.None)
                .Forget();

            Debug.Log($"Game launched for {data.opponentConnection.ClientId} and {hostConnection.ClientId}");
        }

        private async UniTask<NetworkConnection> WaitHostConnAsync(int timeoutMs = 3000)
        {
            var cts = new CancellationTokenSource(timeoutMs);
            NetworkConnection host = null;

            await UniTask.WaitUntil(
                () => _hostConnectionProvider.TryGetConnection(out host),
                cancellationToken: cts.Token
            );

            return host;
        }
    }
}