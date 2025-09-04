using System;
using System.Collections.Generic;
using System.Threading;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Cysharp.Threading.Tasks;
using Game.States;
using Game.User;
using Multiplayer.Contracts;
using Zenject;

namespace Multiplayer.Client.States
{
    public class RoundResultSubstate: GameSubstate<RoundResult>
    {
        private List<UserRoundModel> _roundModels;
        private IWindowsController _windowsController;
        private UserRoundModel _userRoundModel;
        private UserRoundModel _opponentRoundModel;

        public RoundResultSubstate(
            IGameSubstateResolver substateResolverFactory,
            [Inject(Id = GameSubstatesFacade.ROUND_MODELS_ALIAS)] List<UserRoundModel> roundModels,
            [Inject(Id = UserModelConfig.ID)] UserRoundModel userRoundModel,
            [Inject(Id = UserModelConfig.OPPONENT_ID)] UserRoundModel opponentRoundModel,
            IWindowsController windowsController) : base(substateResolverFactory)
        {
            _opponentRoundModel = opponentRoundModel;
            _userRoundModel = userRoundModel;
            _windowsController = windowsController;
            _roundModels = roundModels;
        }
        
        protected override async UniTask EnterAsync(RoundResult payload, CancellationToken ct)
        {
            _roundModels.UpdateAllModels(payload.WinnerIds);
            var windowPayload = new UIWindowRoundResult.Payload(_userRoundModel, _opponentRoundModel);
            await _windowsController.OpenAsync<UIWindowRoundResult,UIWindowRoundResult.Payload>(windowPayload,ct);
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: ct);
            await SubstateMachine.ChangeStateAsync<RoundClearSubstate>(ct);

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