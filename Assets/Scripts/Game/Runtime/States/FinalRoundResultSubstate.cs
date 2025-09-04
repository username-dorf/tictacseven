using System;
using System.Collections.Generic;
using System.Threading;
using Core.StateMachine;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Cysharp.Threading.Tasks;
using Game.User;
using Zenject;

namespace Game.States
{
    public class FinalRoundResultSubstateGameSubstate: GameSubstate<FinalRoundResultSubstateGameSubstate.Payload>
    {
        private List<UserRoundModel> _roundModels;
        private IWindowsController _windowsController;
        private UserRoundModel.Provider _userRoundModelProvider;
        private AIUserRoundModel.Provider _opponentRoundModelProvider;
        private IStateMachine _mainStateMachine;

        public class Payload
        {
            public List<int> WinnerOwners { get; }

            public Payload(params int[] winnerOwners)
            {
                WinnerOwners = new List<int>(winnerOwners);
            }
        }

        public FinalRoundResultSubstateGameSubstate(
            IStateMachine mainStateMachine,
            IGameSubstateResolver substateResolverFactory,
            [Inject(Id = GameSubstatesFacade.ROUND_MODELS_ALIAS)] List<UserRoundModel> roundModels,
            UserRoundModel.Provider userRoundModelProvider,
            AIUserRoundModel.Provider opponentRoundModelProvider,
            IWindowsController windowsController) : base(substateResolverFactory)
        {
            _mainStateMachine = mainStateMachine;
            _opponentRoundModelProvider = opponentRoundModelProvider;
            _userRoundModelProvider = userRoundModelProvider;
            _windowsController = windowsController;
            _roundModels = roundModels;
        }
        
        protected override async UniTask EnterAsync(Payload payload, CancellationToken ct)
        {
            _roundModels.UpdateAllModels(payload.WinnerOwners);
            var windowPayload = new UIWindowRoundResult.Payload(_userRoundModelProvider.Model,_opponentRoundModelProvider.Model);
            await _windowsController.OpenAsync<UIWindowRoundResult,UIWindowRoundResult.Payload>(windowPayload,ct);
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: ct);
            await _mainStateMachine.ChangeStateAsync<MenuState>(CancellationToken.None);

        }
        public override UniTask ExitAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            
        }
    }
}