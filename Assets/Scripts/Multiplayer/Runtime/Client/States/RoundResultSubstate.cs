using System;
using System.Collections.Generic;
using System.Threading;
using Core.StateMachine;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Cysharp.Threading.Tasks;
using Game.States;
using Game.User;
using Multiplayer.Contracts;
using UniState;
using Zenject;

namespace Multiplayer.Client.States
{
    public class RoundResultSubstate: GameSubstate<RoundResult>
    {
        private LazyInject<List<UserRoundModel>> _roundModels;
        private LazyInject<UserRoundModel> _userRoundModel;
        private LazyInject<UserRoundModel> _opponentRoundModel;
        private LazyInject<IStateProviderDebug> _stateProviderDebug;
        private IWindowsController _windowsController;

        public RoundResultSubstate(
            [Inject(Id = GameSubstatesFacade.ROUND_MODELS_ALIAS)] LazyInject<List<UserRoundModel>> roundModels,
            [Inject(Id = UserModelConfig.ID)] LazyInject<UserRoundModel> userRoundModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] LazyInject<UserRoundModel> opponentRoundModel,
            [InjectOptional] LazyInject<IStateProviderDebug> stateProviderDebug,
            IWindowsController windowsController)
        {
            _stateProviderDebug = stateProviderDebug;
            _opponentRoundModel = opponentRoundModel;
            _userRoundModel = userRoundModel;
            _windowsController = windowsController;
            _roundModels = roundModels;
        }
        
        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            _stateProviderDebug?.Value?.ChangeState(this);
            
            _roundModels.Value.UpdateAllModels(Payload.WinnerIds);
            var windowPayload = new UIWindowRoundResult.Payload(_userRoundModel.Value, _opponentRoundModel.Value);
            await _windowsController.OpenAsync<UIWindowRoundResult,UIWindowRoundResult.Payload>(windowPayload,token);
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: token);
            return Transition.GoTo<RoundClearSubstate>();
        }
    }
}