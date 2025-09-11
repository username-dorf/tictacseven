using System;
using System.Collections.Generic;
using System.Threading;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Cysharp.Threading.Tasks;
using Game.User;
using UniState;
using Zenject;

namespace Game.States
{
    public class RoundResultSubstate : GameSubstate<RoundResultSubstate.PayloadModel>
    {
        private LazyInject<List<UserRoundModel>> _roundModels;
        private IWindowsController _windowsController;
        private LazyInject<UserRoundModel> _userRoundModel;
        private LazyInject<UserRoundModel> _opponentRoundModel;

        public struct PayloadModel
        {
            public List<int> WinnerOwners { get; }

            public PayloadModel(params int[] winnerOwners)
            {
                WinnerOwners = new List<int>(winnerOwners);
            }
        }

        public RoundResultSubstate(
            [Inject(Id = GameSubstatesFacade.ROUND_MODELS_ALIAS)] LazyInject<List<UserRoundModel>> roundModels,
            [Inject(Id = UserModelConfig.ID)] LazyInject<UserRoundModel> userRoundModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<UserRoundModel> opponentRoundModel,
            IWindowsController windowsController)
        {
            _opponentRoundModel = opponentRoundModel;
            _userRoundModel = userRoundModel;
            _windowsController = windowsController;
            _roundModels = roundModels;
        }
        
        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _roundModels.Value.UpdateAllModels(Payload.WinnerOwners);
            var windowPayload = new UIWindowRoundResult.Payload(_userRoundModel.Value,_opponentRoundModel.Value);
            await _windowsController.OpenAsync<UIWindowRoundResult,UIWindowRoundResult.Payload>(windowPayload,token);
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: token);
            return Transition.GoTo<RoundClearSubstate>();

        }
    }
}