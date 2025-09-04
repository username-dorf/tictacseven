using System;
using System.Threading;
using Core.User;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UniRx;
using UnityEngine;
using Zenject;

namespace Core.UI.Components
{
    public class UIRoundResultFactoryInstaller : Installer<UIRoundResultFactoryInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindFactory<IUserRoundModel, UIRoundResultView.ViewModel,
                    UIRoundResultView.ViewModel.UserFactory>()
                .FromMethod((ctx, model) =>
                    new UIRoundResultView.ViewModel(
                        model,
                        ctx.Resolve<ProfileSpriteSetsProvider>()));

            Container
                .BindFactory<IAIUserRoundModel, UIRoundResultView.ViewModel,
                    UIRoundResultView.ViewModel.AIUserFactory>()
                .FromMethod((ctx, model) =>
                    new UIRoundResultView.ViewModel(
                        model,
                        ctx.Resolve<ProfileSpriteSetsProvider>()));

            Container.Bind<UIRoundResultView.Factory>()
                .AsSingle();
        }
    }
    public class UIRoundResultView : MonoBehaviour
    {
        [field: SerializeField] public UIUserProfileView userProfileView { get; private set; }
        [field: SerializeField] public UIVariableDotViewGroup DotViewsGroup { get; private set; }
        [SerializeField] private bool inverted;
        private Vector3 _originScale;

        public void Initialize(ViewModel viewModel)
        {
            if(viewModel is null)
                return;
            userProfileView.Initialize(viewModel.ProfileSprite,viewModel.Username);
            
            viewModel.RoundResults
                .ObserveAdd()
                .Subscribe(@event=> DotViewsGroup.SetLastStateAsync(@event.Value, CancellationToken.None))
                .AddTo(this);

            viewModel.AwaitingTurn
                .DistinctUntilChanged()
                .Subscribe(value => SetAwaitingTurn(value,inverted))
                .AddTo(this);
        }

        private void OnDestroy()
        {
            StopAnimation();
        }

        public void PrepareAnimation()
        {
            _originScale = transform.localScale;
            transform.localScale *= 1.18f;
            transform.localEulerAngles = new Vector3(0f, 0f, 5);
        }

        public async UniTask SetAwaitingTurn(bool value, bool invert)
        {
            var scale = value ? 1 : 0.6f;
            if (!invert)
            {
                await ((RectTransform) transform).ScaleKeepLeft(scale, 0.2f);
            }
            else
            {
                await ((RectTransform) transform).ScaleKeepRight(scale, 0.2f);
            }
        }
        public async UniTask PlayAnimationAsync(CancellationToken token)
        {
            var t1 = 0.12f;
            var t2 = 0.11f;
            var t3 = 0.10f; 
            var uvDuration = t1 + t2 + t3;
            var seq = Sequence.Create()

                .Group(Tween.LocalEulerAngles(transform, new Vector3(0,0,5f), Vector3.zero,
                    uvDuration, Ease.OutCubic))
                .Chain(Tween.Scale(transform, _originScale * 0.96f, t1, Ease.InCubic))
                .Chain(Tween.Scale(transform, _originScale * 1.04f, t2, Ease.OutBack))
                .Chain(Tween.Scale(transform, _originScale, t3, Ease.OutQuad))
                .ToUniTask(cancellationToken: token);
            try
            {
                await seq;
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        public void StopAnimation()
        {
            Tween.StopAll(transform);
        }

        public class ViewModel : IDisposable
        {
            public ReactiveProperty<Sprite> ProfileSprite { get; private set; }
            public ReadOnlyReactiveProperty<string> Username { get; private set; }
            public IReadOnlyReactiveCollection<bool> RoundResults { get; private set; }
            
            public ReadOnlyReactiveProperty<bool> AwaitingTurn { get; private set; }
            
            public ViewModel(IUserRoundModel model, 
                [Inject] ProfileSpriteSetsProvider profileSpritesProvider)
            {
                var userProfileAssetId = model.ProfileAssetId.Value;
                var userProfileSprites = profileSpritesProvider.GetAsset(userProfileAssetId);
                var userAvatarEmotion = ProfileEmotion.Default;
                ProfileSprite = new ReactiveProperty<Sprite>(userProfileSprites.GetEmotionSprite(userAvatarEmotion));
                Username = new ReadOnlyReactiveProperty<string>(model.UserModel.Nickname);
                RoundResults = model.RoundResults;
                AwaitingTurn = model.AwaitingTurn.ToReadOnlyReactiveProperty();
            }

            public ViewModel(IAIUserRoundModel model,
                [Inject] ProfileSpriteSetsProvider profileSpritesProvider)
            {
                var opponentAvatarEmotion = ProfileEmotion.Default;
                var opponentProfileSprites = profileSpritesProvider.GetAsset(model.ProfileAssetId.Value);
                ProfileSprite = new ReactiveProperty<Sprite>(opponentProfileSprites.GetEmotionSprite(opponentAvatarEmotion));
                Username = new ReadOnlyReactiveProperty<string>(model.UserModel.Nickname);
                RoundResults = model.RoundResults;
                AwaitingTurn = model.AwaitingTurn.ToReadOnlyReactiveProperty();
            }

            public void Dispose()
            {
                ProfileSprite?.Dispose();
                Username?.Dispose();
            }

            public class UserFactory : PlaceholderFactory<IUserRoundModel,ViewModel>
            {
                
            }
            public class AIUserFactory : PlaceholderFactory<IAIUserRoundModel,ViewModel>
            {
                
            }
        }
        

        public class Factory
        {
            private DiContainer _diContainer;
            private ViewModel.UserFactory _userFactory;
            private ViewModel.AIUserFactory _aiUserFactory;

            public Factory(ViewModel.UserFactory userFactory,ViewModel.AIUserFactory aiUserFactory)
            {
                _aiUserFactory = aiUserFactory;
                _userFactory = userFactory;
            }
            
            public ViewModel BindExisting(IUserRoundModel model, UIRoundResultView view)
            {
                var viewModel = _userFactory.Create(model);
                view.Initialize(viewModel);
                return viewModel;
            }
            public ViewModel BindExisting(IAIUserRoundModel model, UIRoundResultView view)
            {
                var viewModel = _aiUserFactory.Create(model);
                view.Initialize(viewModel);
                return viewModel;
            }

            public ViewModel Create(IUserRoundModel model, RectTransform parent)
            {
                throw new NotImplementedException();
            }
        }
    }
}