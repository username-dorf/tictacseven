using System;
using System.Collections.Generic;
using System.Threading;
using Core.AssetProvider;
using Core.Common;
using Cysharp.Threading.Tasks;
using Menu.Runtime.UI;
using Menu.UI;
using Menu.UIWorld;
using UniRx;
using UnityEngine;

namespace Menu.Runtime.UIWorld
{
    public class MenuFieldView: MonoBehaviour
    {
        [field: SerializeField] public UIWorldButtonView ClassicButtonView { get; protected set; }
        [field: SerializeField] public UIWorldButtonView ArcadeButtonView { get; protected set; }
        [field: SerializeField] public UIWorldButtonView MultiplayerButtonView { get; protected set; }
        
        [field: SerializeField] public UIWorldButtonView CreateHostButtonView { get; protected set; }
        [field: SerializeField] public UIWorldButtonView ConnectClientButtonView { get; protected set; }
        [field: SerializeField] public UIWorldButtonView BackButtonView { get; protected set; }
        
        [SerializeField] private Transform meshTransform;

        private void Awake()
        {
            PrepareAnimation();
        }

        public void Initialize(ViewModel viewModel)
        {
            viewModel.State
                .Skip(1)
                .Subscribe(OnViewStateChanged)
                .AddTo(this);
            
            MultiplayerButtonView.Initialize(new ModeButtonViewModel(ct => viewModel.CallMultiplayerButtonsSet()));
            BackButtonView.Initialize(new ModeButtonViewModel(ct => viewModel.CallBaseButtonsSet()));
        }

        public async UniTask PlayScaleAsync(CancellationToken ct)
        {
            try
            {
                await meshTransform.ScaleBounceAllAxes(duration: 0.35f)
                    .ToUniTask(cancellationToken: ct);
                
                PlayBaseButtonsAppearAsync(ct)
                    .Forget();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        private void PrepareAnimation()
        {
            ClassicButtonView.transform.localScale = Vector3.one;
            
            ClassicButtonView.gameObject.SetActive(false);
            ArcadeButtonView.gameObject.SetActive(false);
            MultiplayerButtonView.gameObject.SetActive(false);
            
            CreateHostButtonView.gameObject.SetActive(false);
            ConnectClientButtonView.gameObject.SetActive(false);
            BackButtonView.gameObject.SetActive(false);
        }

        private async UniTask PlayBaseButtonsAppearAsync(CancellationToken ct)
        {
            await CreateButtonsHideAsync(ct,CreateHostButtonView, ConnectClientButtonView, BackButtonView)
                .ContinueWith(() =>
                    CreateButtonsAppearAsync(ct, ClassicButtonView, ArcadeButtonView, MultiplayerButtonView));
        }
        private async UniTask PlayMultiplayerButtonsAppearAsync(CancellationToken ct)
        {
            await CreateButtonsHideAsync(ct, ClassicButtonView, ArcadeButtonView, MultiplayerButtonView)
                .ContinueWith(() =>
                    CreateButtonsAppearAsync(ct, CreateHostButtonView, ConnectClientButtonView, BackButtonView));
        }

        private async UniTask CreateButtonsAppearAsync(CancellationToken ct, params UIWorldButtonView[] buttons)
        {
            var initSpawnScaleFactor = 0; 
            var animations = new List<Func<UniTask>>();
            foreach (var view in buttons)
            {
                view.gameObject.SetActive(true);
                Func<UniTask> animation = ()=> view.transform
                    .ScaleBounceAllAxes(spawnScaleFactor:initSpawnScaleFactor)
                    .ToUniTask(cancellationToken:ct);
                animations.Add(animation);
            }

            try
            {
                await UniTask.WhenAll(animations.Select(x=>x.Invoke()));
            }
            catch (OperationCanceledException e)
            {
                
            }
        }

        private async UniTask CreateButtonsHideAsync(CancellationToken ct, params UIWorldButtonView[] buttons)
        {
            var animations = new List<Func<UniTask>>();
            foreach (var view in buttons)
            {
                Func<UniTask> animation = ()=> view.transform
                    .ScaleHideBounceAllAxes(duration: 0.2f, onHidden: tr=>{tr.gameObject.SetActive(false);})
                    .ToUniTask(cancellationToken:ct);
                animations.Add(animation);
            }

            try
            {
                await UniTask.WhenAll(animations.Select(x=>x.Invoke()));
            }
            catch (OperationCanceledException e)
            {
                
            }
        }

        private void OnViewStateChanged(State state)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            switch (state)
            {
                case State.Default:
                    PlayBaseButtonsAppearAsync(cts.Token)
                        .Forget();
                    break;
                case State.Multiplayer:
                    PlayMultiplayerButtonsAppearAsync(cts.Token)
                        .Forget();
                    break;
            }
        }

        public enum State
        {
            Default=0,
            Multiplayer
        }
        public class ViewModel
        {
            public ReactiveProperty<State> State { get; private set; }

            public ViewModel()
            {
                State = new ReactiveProperty<State>(MenuFieldView.State.Default);
            }

            public UniTask CallMultiplayerButtonsSet()
            {
                State.Value = MenuFieldView.State.Multiplayer;
                return UniTask.CompletedTask;
            }

            public UniTask CallBaseButtonsSet()
            {
                State.Value = MenuFieldView.State.Default;
                return UniTask.CompletedTask;
            }
        }
    }
    
    public class MenuFieldViewFactory : IDisposable
    {
        private AssetProvider _assetProvider;
        private UIMenuController _uiMenuController;

        public MenuFieldViewFactory(UIMenuController uiMenuController)
        {
            _uiMenuController = uiMenuController;
            _assetProvider = new AssetProvider();
        }
        
        public async UniTask<MenuFieldView> CreateAsync(CancellationToken cancellationToken)
        {
            await _assetProvider.LoadAsset(cancellationToken,AssetProvider.AssetPath);
            var prefab = _assetProvider.GetAsset();
            if(prefab is null)
                return null;
            var view = GameObject.Instantiate(prefab);
            if (view is null)
                throw new Exception("Failed to instantiate FieldView from prefab");
            _uiMenuController.BindButtonViews(view);
            var viewModel = new MenuFieldView.ViewModel();
            view.Initialize(viewModel);

            return view;
        }
        
        public void Dispose()
        {
            _assetProvider?.Dispose();
        }

        private class AssetProvider : AssetProvider<MenuFieldView>
        {
            public const string AssetPath = "MenuFieldView";
        }
    }
}