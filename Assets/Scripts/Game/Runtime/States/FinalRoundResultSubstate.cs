using System;
using System.Collections.Generic;
using System.Threading;
using Core.StateMachine;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Cysharp.Threading.Tasks;
using Game.User;
using UniState;
using Zenject;

namespace Game.States
{
    public class FinalRoundResultSubstateGameSubstate: GameSubstate<FinalRoundResultSubstateGameSubstate.PayloadModel>
    {
        private LazyInject<List<UserRoundModel>> _roundModels;
        private IWindowsController _windowsController;
        private LazyInject<UserRoundModel.Provider> _userRoundModelProvider;
        private LazyInject<AIUserRoundModel.Provider> _opponentRoundModelProvider;
        private IStateMachine _mainStateMachine;
        private ManualTransitionTrigger<MenuState> _menuTransitionTrigger;

        public struct PayloadModel
        {
            public List<int> WinnerOwners { get; }

            public PayloadModel(params int[] winnerOwners)
            {
                WinnerOwners = new List<int>(winnerOwners);
            }
        }

        public FinalRoundResultSubstateGameSubstate(
            ManualTransitionTrigger<MenuState> menuTransitionTrigger,
            [Inject(Id = GameSubstatesFacade.ROUND_MODELS_ALIAS)] LazyInject<List<UserRoundModel>> roundModels,
            LazyInject<UserRoundModel.Provider> userRoundModelProvider,
            LazyInject<AIUserRoundModel.Provider> opponentRoundModelProvider,
            IWindowsController windowsController)
        {
            _menuTransitionTrigger = menuTransitionTrigger;
            _opponentRoundModelProvider = opponentRoundModelProvider;
            _userRoundModelProvider = userRoundModelProvider;
            _windowsController = windowsController;
            _roundModels = roundModels;
        }

        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _roundModels.Value.UpdateAllModels(Payload.WinnerOwners);
            var windowPayload =
                new UIWindowRoundResult.Payload(_userRoundModelProvider.Value.Model, _opponentRoundModelProvider.Value.Model);
            await _windowsController.OpenAsync<UIWindowRoundResult, UIWindowRoundResult.Payload>(windowPayload, token);
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: token);
            
            _menuTransitionTrigger.Continue();
            return Transition.GoToExit();
        }
    }
}