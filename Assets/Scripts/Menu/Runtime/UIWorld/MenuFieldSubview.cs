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
    public class MenuFieldSubview : MonoBehaviour
    {
        [field: SerializeField] public UIWorldButtonView SettingsButton { get; protected set; }
        [field: SerializeField] public UIWorldButtonView ProfileButton { get; protected set; }
        [field: SerializeField] public UIWorldButtonView SocialButton { get; protected set; }
        [SerializeField] private Transform meshTransform;

        private void Awake()
        {
            PrepareAnimation();
        }

        public async UniTask PlayScaleAsync(CancellationToken ct)
        {
            try
            {
                await meshTransform.ScaleFromCorner(new Vector2Int(1,1),0.3f)
                    .ToUniTask(cancellationToken:ct);
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
            SettingsButton.gameObject.SetActive(false);
            ProfileButton.gameObject.SetActive(false);
            SocialButton.gameObject.SetActive(false);
        }
        
        private async UniTask PlayButtonAppearAsync(CancellationToken ct)
        {
            var initSpawnScaleFactor = 0; 
            SettingsButton.gameObject.SetActive(true);
            ProfileButton.gameObject.SetActive(true);
            SocialButton.gameObject.SetActive(true);
            
            Func<UniTask> settingsAppearAsync = ()=> SettingsButton.transform
                .ScaleBounceAllAxes(spawnScaleFactor:initSpawnScaleFactor)
                .ToUniTask(cancellationToken:ct);
            
            Func<UniTask> profileAppearAsync = ()=> ProfileButton.transform
                .ScaleBounceAllAxes(spawnScaleFactor:initSpawnScaleFactor)
                .ToUniTask(cancellationToken:ct);
            
            Func<UniTask> socialAppearAsync = ()=> SocialButton.transform
                .ScaleBounceAllAxes(spawnScaleFactor:initSpawnScaleFactor)
                .ToUniTask(cancellationToken:ct);
           
            try
            {
                await UniTask.WhenAll(settingsAppearAsync(),profileAppearAsync(), socialAppearAsync());
            }
            catch (OperationCanceledException)
            {
                
            }
        }
    }
    
    public class MenuFieldSubviewFactory : IDisposable
    {
        private AssetProvider _assetProvider;
        private UIMenuController _uiMenuController;

        public MenuFieldSubviewFactory(UIMenuController uiMenuController)
        {
            _uiMenuController = uiMenuController;
            _assetProvider = new AssetProvider();
        }
        
        public async UniTask<MenuFieldSubview> CreateAsync(CancellationToken cancellationToken)
        {
            await _assetProvider.LoadAsset(cancellationToken,AssetProvider.AssetPath);
            var prefab = _assetProvider.GetAsset();
            if(prefab is null)
                return null;
            var view = GameObject.Instantiate(prefab);
            if (view is null)
                throw new Exception("Failed to instantiate FieldView from prefab");
            view.transform.position += new Vector3(1f, 0, 1f);
            _uiMenuController.BindSubviewButtonViews(view);
            return view;
        }
        
        public void Dispose()
        {
            _assetProvider?.Dispose();
        }

        private class AssetProvider : AssetProvider<MenuFieldSubview>
        {
            public const string AssetPath = "MenuFieldSubview";
        }
    }
}