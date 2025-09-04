using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.User;
using UnityEngine;
using Zenject;

namespace Game.States
{
    public class RoundResultSubstate : GameSubstate<RoundResultSubstate.Payload>
    {
        private List<UserRoundModel> _roundModels;

        public class Payload
        {
            public int WinnerOwner { get; }

            public Payload(int winnerOwner)
            {
                WinnerOwner = winnerOwner;
            }
        }

        public RoundResultSubstate(IGameSubstateResolver substateResolverFactory,
            [Inject(Id = GameSubstateSettings.ROUND_MODELS_ALIAS)] List<UserRoundModel> roundModels) : base(substateResolverFactory)
        {
            _roundModels = roundModels;
        }
        
        protected override async UniTask EnterAsync(Payload payload, CancellationToken ct)
        {
            Debug.Log($"RoundResultSubstate: EnterAsync with winner owner {payload.WinnerOwner}");
            UpdateRoundModels(payload.WinnerOwner);
            await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: ct);
            await SubstateMachine.ChangeStateAsync<RoundClearSubstate>();

        }
        public override UniTask ExitAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
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