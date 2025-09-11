using System;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Multiplayer.Contracts;
using UniRx;
using UniState;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Server.States
{
    public class ClientTurnSubstate : ServerSubstate
    {
        private const int TIMEOUT_SECONDS = 30;
        
        private IServerActiveClientProvider _activeClientProvider;
        private LazyInject<IStateProviderDebug> _stateProviderDebug;

        private ReactiveCommand<ClientTurnResponse> _turnDoneResponseReceived;


        public ClientTurnSubstate(
            IServerActiveClientProvider activeClientProvider,
            [InjectOptional] LazyInject<IStateProviderDebug> stateProviderDebug)
        {
            _stateProviderDebug = stateProviderDebug;
            _activeClientProvider = activeClientProvider;
            _turnDoneResponseReceived = new ReactiveCommand<ClientTurnResponse>();
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _stateProviderDebug?.Value?.ChangeState(this);
            AddDisposables();

            InstanceFinder.ServerManager.RegisterBroadcast<ClientTurnResponse>(OnClientTurnDone);

            var clientId = _activeClientProvider.ActiveClientId.Value;
            StartTurn(clientId, TIMEOUT_SECONDS);


            using var raceCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var timeOutTask = WaitTurnTimeOutAsync(clientId, raceCts.Token);
            var turnDoneTask = WaitTurnDoneAsync(raceCts.Token);

            var (i, timeoutResult, turnDoneResult) = await UniTask.WhenAny(timeOutTask, turnDoneTask);

            raceCts.Cancel();

            return i switch
            {
                0 => timeoutResult,
                1 => turnDoneResult,
                _ => Transition.GoToExit()
            };
        }

        public async UniTask<StateTransitionInfo> WaitTurnTimeOutAsync(
            string clientId,
            CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(TIMEOUT_SECONDS),
                DelayType.Realtime, PlayerLoopTiming.Update, token);
            return OnTurnTimeout(clientId);
        }

        public async UniTask<StateTransitionInfo> WaitTurnDoneAsync(
            CancellationToken token)
        {
            var result = await _turnDoneResponseReceived
                .First()
                .ToUniTask(cancellationToken: token);
            return Transition.GoTo<FieldSyncSubstate, ClientTurnResponse>(result);
        }

        public override UniTask Exit(CancellationToken token)
        {
            InstanceFinder.ServerManager.UnregisterBroadcast<ClientTurnResponse>(OnClientTurnDone);
            return base.Exit(token);
        }

        private StateTransitionInfo OnTurnTimeout(string clientId)
        {
            InstanceFinder.ServerManager.Broadcast(new ClientTurnTimeout {OffenderClientId = clientId});
            _activeClientProvider.ChangeActiveClientId();
            return Transition.GoTo<ClientTurnSubstate>();
        }

        private void StartTurn(string clientId, int timeoutSeconds)
        {
            var now = ServerClock.NowTicks();
            var deadline = ServerClock.AddSeconds(now, timeoutSeconds);

            InstanceFinder.ServerManager.Broadcast(new ClientTurn
            {
                ActiveClientId = clientId,
                ServerNowTicks = now,
                DeadlineTicks = deadline,
                ClockFrequency = ServerClock.Frequency
            });
        }

        private void OnClientTurnDone(NetworkConnection conn, ClientTurnResponse response, Channel channel)
        {
            _turnDoneResponseReceived?.Execute(response);
        }

        private void AddDisposables()
        {
            _turnDoneResponseReceived.AddTo(Disposables);
            Disposables.Add(() =>
                InstanceFinder.ServerManager.UnregisterBroadcast<ClientTurnResponse>(OnClientTurnDone));
        }
    }
}