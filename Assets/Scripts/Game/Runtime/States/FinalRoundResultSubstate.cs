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
            public int WinnerOwner { get; }

            public Payload(int winnerOwner)
            {
                WinnerOwner = winnerOwner;
            }
        }

        public FinalRoundResultSubstateGameSubstate(
            IStateMachine mainStateMachine,
            IGameSubstateResolver substateResolverFactory,
            [Inject(Id = GameSubstateSettings.ROUND_MODELS_ALIAS)] List<UserRoundModel> roundModels,
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
            UpdateRoundModels(payload.WinnerOwner);
            var windowPayload = new UIWindowRoundResult.Payload(payload.WinnerOwner,_userRoundModelProvider.Model,_opponentRoundModelProvider.Model);
            await _windowsController.OpenAsync<UIWindowRoundResult,UIWindowRoundResult.Payload>(windowPayload,ct);
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: ct);
            await _mainStateMachine.ChangeStateAsync<MenuState>(CancellationToken.None);

        }
        public override UniTask ExitAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            
        }

        private void UpdateRoundModels(int winnerOwner)
        {
            foreach (var model in _roundModels)
            {
                var isWinnerModel = model.Owner == winnerOwner;
                model.SetRoundResult(isWinnerModel);
            }
        }
    }
}