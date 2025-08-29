using System;
using System.Threading;
using Core.AssetProvider;
using Core.Common;
using Cysharp.Threading.Tasks;
using Menu.Runtime.UI;
using Menu.UIWorld;
using UnityEngine;

namespace Menu.Runtime.UIWorld
{
    public class MenuFieldView: MonoBehaviour
    {
        [field: SerializeField] public UIWorldButtonView ClassicButtonView { get; protected set; }
        [field: SerializeField] public UIWorldButtonView ArcadeButtonView { get; protected set; }
        [field: SerializeField] public UIWorldButtonView MultiplayerButtonView { get; protected set; }
        
        [SerializeField] private Transform meshTransform;

        private void Awake()
        {
            PrepareAnimation();
        }

        public async UniTask PlayScaleAsync(CancellationToken ct)
        {
            try
            {
                await meshTransform.ScaleBounceAllAxes(duration: 0.35f)
                    .ToUniTask(cancellationToken: ct);
                
                PlayButtonAppearAsync(ct)
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
        }

        private async UniTask PlayButtonAppearAsync(CancellationToken ct)
        {
            var initSpawnScaleFactor = 0; 
            ClassicButtonView.gameObject.SetActive(true);
            ArcadeButtonView.gameObject.SetActive(true);
            MultiplayerButtonView.gameObject.SetActive(true);
            
            Func<UniTask> classicAppearAsync = ()=> ClassicButtonView.transform
                .ScaleBounceAllAxes(spawnScaleFactor:initSpawnScaleFactor)
                .ToUniTask(cancellationToken:ct);
            
            Func<UniTask> arcadeAppearAsync = ()=> ArcadeButtonView.transform
                .ScaleBounceAllAxes(spawnScaleFactor:initSpawnScaleFactor)
                .ToUniTask(cancellationToken:ct);
            
            Func<UniTask> multiplayerAppearAsync = ()=> MultiplayerButtonView.transform
                .ScaleBounceAllAxes(spawnScaleFactor:initSpawnScaleFactor)
                .ToUniTask(cancellationToken:ct);
            try
            {
                await UniTask.WhenAll(classicAppearAsync(), arcadeAppearAsync(), multiplayerAppearAsync());
            }
            catch (OperationCanceledException)
            {
                
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