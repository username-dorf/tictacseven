using System.Threading;
using Core.StateMachine;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.States;
using Multiplayer.Contracts;
using UniRx;
using UniState;
using Zenject;
using Channel = FishNet.Transporting.Channel;

namespace Multiplayer.Client.States
{
    public class WaitTurnSubstate : GameSubstate
    {
        private LazyInject<IStateProviderDebug> _stateProviderDebug;
        private LazyInject<IUserPreferencesProvider> _userPreferencesProvider;
        
        private ReactiveCommand<ClientTurn> _onTurnReceived;
        private ReactiveCommand<ClientFieldSync> _onSyncReceived;
       

        public WaitTurnSubstate([InjectOptional] LazyInject<IStateProviderDebug> stateProviderDebug,
            LazyInject<IUserPreferencesProvider> userPreferencesProvider)
        {
            _userPreferencesProvider = userPreferencesProvider;
            _stateProviderDebug = stateProviderDebug;
            _onTurnReceived = new ReactiveCommand<ClientTurn>();
            _onSyncReceived = new ReactiveCommand<ClientFieldSync>();
        }
        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _stateProviderDebug?.Value?.ChangeState(this);
            AddDisposables();

            InstanceFinder.ClientManager.RegisterBroadcast<ClientTurn>(OnTurnReceived);
            InstanceFinder.ClientManager.RegisterBroadcast<ClientFieldSync>(OnSyncReceived);

            
            using var raceCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var syncTask  = WaitSyncReceivedAsync(raceCts.Token);
            var turnTask = WaitTurnReceivedAsync(_userPreferencesProvider.Value.Current.User.Id, raceCts.Token);

            var (i, syncResult, turnResult) = await UniTask.WhenAny(syncTask, turnTask);

            raceCts.Cancel();
            
            return i switch
            {
                0 => syncResult,                
                1 => turnResult,                
                _ => Transition.GoToExit()  
            };
        }

        
        public override UniTask Exit(CancellationToken token)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientFieldSync>(OnSyncReceived);
            InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurn>(OnTurnReceived);
            return base.Exit(token);
        }

        private void OnTurnReceived(ClientTurn response, Channel channel)
        {
            _onTurnReceived?.Execute(response);
        }
        private async UniTask<StateTransitionInfo> WaitTurnReceivedAsync(string myId,
            CancellationToken token)
        {
            var payload = await _onTurnReceived
                .Where(t => t.ActiveClientId == myId) 
                .First()
                .ToUniTask(cancellationToken: token);
            return Transition.GoTo<TurnSubstate,ClientTurn>(payload);
        }
        private void OnSyncReceived(ClientFieldSync arg1, Channel arg2)
        {
            _onSyncReceived?.Execute(arg1);
        }
        private async UniTask<StateTransitionInfo> WaitSyncReceivedAsync(
            CancellationToken token)
        {
            var payload = await _onSyncReceived
                .First()
                .ToUniTask(cancellationToken: token);
            return Transition.GoTo<ServerSyncSubstate,ClientFieldSync>(payload);
        }
        
        private void AddDisposables()
        {
            Disposables.Add(_onTurnReceived);
            Disposables.Add(_onSyncReceived);
            Disposables.Add(() => InstanceFinder.ClientManager.UnregisterBroadcast<ClientTurn>(OnTurnReceived));
            Disposables.Add(() => InstanceFinder.ClientManager.UnregisterBroadcast<ClientFieldSync>(OnSyncReceived));
        }
    }
}