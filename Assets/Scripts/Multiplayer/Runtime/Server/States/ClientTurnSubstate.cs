using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Multiplayer.Contracts;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Server.States
{
    public class ClientTurnSubstate : ServerSubstate
    {
        private const int TIMEOUT_SECONDS = 30;

        private CancellationTokenSource _lifetimeCts;
        private CancellationTokenSource _turnCts;
        private bool _disposed;

        private readonly IServerActiveClientProvider _activeClientProvider;

        public ClientTurnSubstate(
            IServerActiveClientProvider activeClientProvider,
            IServerSubstateResolver substateResolverFactory)
            : base(substateResolverFactory)
        {
            _activeClientProvider = activeClientProvider;
        }

        public override UniTask EnterAsync(CancellationToken ct)
        {
            _lifetimeCts?.Cancel();
            _lifetimeCts?.Dispose();
            _lifetimeCts = new CancellationTokenSource();

            InstanceFinder.ServerManager.RegisterBroadcast<ClientTurnResponse>(OnClientTurnDone);

            var activeClientId = _activeClientProvider.ActiveClientId.Value;
            StartTurn(activeClientId, TIMEOUT_SECONDS);

            return UniTask.CompletedTask;
        }

        public override UniTask ExitAsync(CancellationToken ct)
        {
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientTurnResponse>(OnClientTurnDone);

            CancelAndDisposeTurn();
            _lifetimeCts?.Cancel();
            _lifetimeCts?.Dispose();
            _lifetimeCts = null;

            return UniTask.CompletedTask;
        }

        private void StartTurn(string clientId, int timeoutSeconds)
        {
            if (_disposed || _lifetimeCts == null || _lifetimeCts.IsCancellationRequested)
                return;

            CancelAndDisposeTurn();

            _turnCts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeCts.Token);
            var token = _turnCts.Token;

            var now = ServerClock.NowTicks();
            var deadline = ServerClock.AddSeconds(now, timeoutSeconds);

            InstanceFinder.ServerManager.Broadcast(new ClientTurn
            {
                ActiveClientId = clientId,
                ServerNowTicks = now,
                DeadlineTicks = deadline,
                ClockFrequency = ServerClock.Frequency
            });

            TurnTimeoutLoopAsync(clientId, timeoutSeconds, token)
                .Forget();
        }

        private async UniTask TurnTimeoutLoopAsync(string clientId, int timeoutSeconds, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds),
                    DelayType.Realtime, PlayerLoopTiming.Update, token);

                if (token.IsCancellationRequested || _disposed) return;

                InstanceFinder.ServerManager.Broadcast(new ClientTurnTimeout {OffenderClientId = clientId});

                OnClientTimeout();
                StartTurn(_activeClientProvider.ActiveClientId.Value, TIMEOUT_SECONDS);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void OnClientTimeout()
        {
            _activeClientProvider.ChangeActiveClientId();
        }

        private void OnClientTurnDone(NetworkConnection conn, ClientTurnResponse response, Channel channel)
        {
            CancelAndDisposeTurn();

            SubstateMachine.ChangeStateAsync<FieldSyncSubstate, ClientTurnResponse>(response, CancellationToken.None)
                .Forget();
        }

        private void CancelAndDisposeTurn()
        {
            var cts = _turnCts;
            _turnCts = null;
            if (cts == null) return;

            try
            {
                if (!cts.IsCancellationRequested) cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            cts.Dispose();
        }

        public override void Dispose()
        {
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientTurnResponse>(OnClientTurnDone);

            _disposed = true;
            CancelAndDisposeTurn();
            _lifetimeCts?.Cancel();
            _lifetimeCts?.Dispose();
            _lifetimeCts = null;
        }
    }
}