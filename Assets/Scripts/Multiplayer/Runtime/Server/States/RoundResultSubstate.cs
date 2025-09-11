using System.Collections.Generic;
using System.Threading;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using Game.Field;
using Multiplayer.Connection;
using Multiplayer.Contracts;
using UniRx;
using UniState;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Server.States
{
    public class RoundResultSubstate : ServerSubstate<RoundResultSubstate.PayloadModel>
    {
        private ReactiveProperty<int> _repliedClientsCount;
        private HashSet<int> _whoReplied;

        private LazyInject<FieldModel> _fieldModel;
        private LazyInject<IServerClientsProvider> _clientsProvider;
        private LazyInject<IStateProviderDebug> _stateProviderDebug;

        public struct PayloadModel
        {
            public int PassedRounds;
            public List<string> WinnerIds;
        }

        public RoundResultSubstate(
            LazyInject<FieldModel> fieldModel,
            LazyInject<IServerClientsProvider> clientsProvider,
            [InjectOptional] LazyInject<IStateProviderDebug> stateProviderDebug)
        {
            _stateProviderDebug = stateProviderDebug;
            _clientsProvider = clientsProvider;
            _fieldModel = fieldModel;
            _whoReplied = new HashSet<int>();
            _repliedClientsCount = new ReactiveProperty<int>();
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken ct)
        {
            _stateProviderDebug?.Value?.ChangeState(this);
            AddDisposables();

            InstanceFinder.ServerManager.RegisterBroadcast<RoundResultResponse>(OnRoundResultResponse);

            InstanceFinder.ServerManager.Broadcast(new RoundResult()
            {
                WinnerIds = Payload.WinnerIds
            });

            try
            {
                await _repliedClientsCount
                    .Where(v => v >= ConnectionConfig.MAX_CLIENTS)
                    .First()
                    .ToUniTask(cancellationToken: ct);

                return OnAllRoundResultResponse(Payload.PassedRounds, ct);
            }
            catch
            {
                // ignored
            }

            return Transition.GoToExit();
        }

        private void OnRoundResultResponse(NetworkConnection connection, RoundResultResponse arg2, Channel arg3)
        {
            if (_whoReplied.Add(connection.ClientId))
            {
                _repliedClientsCount.Value = _whoReplied.Count;
            }
        }

        private StateTransitionInfo OnAllRoundResultResponse(int passedRounds, CancellationToken ct)
        {
            if (passedRounds >= FieldConfig.ROUNDS_AMOUNT)
            {
                InstanceFinder.ServerManager.Broadcast(new TerminateSession());
                return Transition.GoToExit();
            }

            _fieldModel?.Value.Drop();
            _clientsProvider?.Value.Drop();
            return Transition.GoTo<ClientTurnSubstate>();
        }

        public override UniTask Exit(CancellationToken ct)
        {
            _whoReplied.Clear();
            InstanceFinder.ServerManager.UnregisterBroadcast<RoundResultResponse>(OnRoundResultResponse);
            return base.Exit(ct);
        }

        private void AddDisposables()
        {
            Disposables.Add(() =>
                InstanceFinder.ServerManager.UnregisterBroadcast<RoundResultResponse>(OnRoundResultResponse));
            _repliedClientsCount.AddTo(Disposables);
        }
    }
}