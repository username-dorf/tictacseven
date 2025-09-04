using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.UI.Components;
using Core.User;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Windows.Views
{
    public class UIWindowRoundResult : WindowView<UIWindowRoundResult.Payload,UIWindowRoundResult.ViewModel>
    {
        [SerializeField] private UIRoundResultView userView;
        [SerializeField] private UIRoundResultView opponentView;
        [SerializeField] private RectTransform roundOverRectTransform;

        private Vector3 _roundOverOriginScale;

        protected override async UniTask OnBeforeBindAsync(CancellationToken ct)
        {
            if (roundOverRectTransform)
            {
                _roundOverOriginScale = roundOverRectTransform.localScale;
                roundOverRectTransform.localScale *= 0.2f;
            }

            if (userView)
                userView.PrepareAnimation();
            
            if (opponentView)
                opponentView.PrepareAnimation();
        }
        protected override async UniTask BindViewAsync(ViewModel viewModel, CancellationToken ct)
        {
            userView.userProfileView.Initialize(viewModel.UserAvatar,viewModel.UserNickname);
            opponentView.userProfileView.Initialize(viewModel.OpponentAvatar,viewModel.OpponentNickname);

            var warmupUserRoundResults = userView.DotViewsGroup
                .WarmUp(viewModel.UserRoundResults[..^1], ct);
            
            var warmupOpponentRoundResults = opponentView.DotViewsGroup
                .WarmUp(viewModel.OpponentRoundResults[..^1], ct);
            
            await UniTask.WhenAll(warmupUserRoundResults, warmupOpponentRoundResults);
            viewModel.AutoCloseAsync(ct).Forget();
        }

        protected override async UniTask PlayOpenAnimationAsync(CancellationToken ct)
        {
            var overshoot = 1.06f;
            var duration = 0.8f;
            var up = duration * 0.55f;
            var down = duration - up;
            var roundOverSeq = Sequence.Create()
                .Chain(Tween.Scale(roundOverRectTransform, _roundOverOriginScale * overshoot, up, Ease.OutCubic))
                .Chain(Tween.Scale(roundOverRectTransform, _roundOverOriginScale, down, Ease.OutBack))
                .ToUniTask(cancellationToken: ct);
            
            
            await UniTask.WhenAll(userView.PlayAnimationAsync(ct), opponentView.PlayAnimationAsync(ct), roundOverSeq);
            var setUserCurrentResult = userView.DotViewsGroup.SetLastStateAsync(VM.UserRoundResults.Last(), ct);
            var setOpponentCurrentResult = opponentView.DotViewsGroup.SetLastStateAsync(VM.OpponentRoundResults.Last(), ct);
            await UniTask.WhenAll(setUserCurrentResult,setOpponentCurrentResult);
        }
        protected override async UniTask OnBeforeCloseAsync(CancellationToken ct)
        {
            Tween.StopAll(roundOverRectTransform);
            userView.StopAnimation();
        }


        public class ViewModel : IViewModel, IPayloadReceiver<Payload>
        {
            private IUserPreferencesProvider _userPreferencesProvider;
            private ProfileSpriteSetsProvider _profileSpritesProvider;
            private IWindowsController _windowsController;
            public ReactiveProperty<Sprite> UserAvatar { get; private set; }
            public ReactiveProperty<string> UserNickname { get; private set; }
            public bool[] UserRoundResults { get; private set; }
            
            public ReactiveProperty<Sprite> OpponentAvatar { get; private set; }
            public ReactiveProperty<string> OpponentNickname { get; private set; }
            public bool[] OpponentRoundResults { get; private set; }
            
            

            public ViewModel(
                IUserPreferencesProvider userPreferencesProvider,
                ProfileSpriteSetsProvider profileSpritesProvider,
                IWindowsController windowsController)
            {
                _windowsController = windowsController;
                _profileSpritesProvider = profileSpritesProvider;
                _userPreferencesProvider = userPreferencesProvider;
            }
            public void SetPayload(Payload payload)
            {
                var userPreferences = _userPreferencesProvider.Current;
                var userProfileAssetId = userPreferences.ProfileAssetId.Value;
                var userProfileSprites = _profileSpritesProvider.GetAsset(userProfileAssetId);
                UserRoundResults = payload.UserRoundModel.RoundResults.ToArray();
                var userAvatarEmotion = UserRoundResults[^1]
                    ? ProfileEmotion.Happy
                    : ProfileEmotion.Sad;
                UserAvatar = new ReactiveProperty<Sprite>(userProfileSprites.GetEmotionSprite(userAvatarEmotion));
                UserNickname = new ReactiveProperty<string>(userPreferences.User.Nickname.Value);

                OpponentRoundResults = payload.OpponentRoundModel.RoundResults.ToArray();
                var opponentAvatarEmotion = OpponentRoundResults[^1]
                    ? ProfileEmotion.Happy
                    : ProfileEmotion.Sad;
                var opponentProfileSprites = _profileSpritesProvider.GetAsset(payload.OpponentRoundModel.ProfileAssetId.Value);
                OpponentAvatar = new ReactiveProperty<Sprite>(opponentProfileSprites.GetEmotionSprite(opponentAvatarEmotion));
                OpponentNickname = new ReactiveProperty<string>(payload.OpponentRoundModel.UserModel.Nickname.Value);
                
            }

            public async UniTask AutoCloseAsync(CancellationToken ct)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: ct);
                    await _windowsController.CloseTopAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            public void Dispose()
            {
                UserAvatar?.Dispose();
                OpponentAvatar?.Dispose();
            }
        }
        
        public class Payload
        {
            public IUserRoundModel UserRoundModel { get; }
            public IUserRoundModel OpponentRoundModel { get; }
            public Payload(IUserRoundModel userRoundModel, IUserRoundModel opponentRoundModel)
            {
                UserRoundModel = userRoundModel;
                OpponentRoundModel = opponentRoundModel;
            }
            
        }
    }
}