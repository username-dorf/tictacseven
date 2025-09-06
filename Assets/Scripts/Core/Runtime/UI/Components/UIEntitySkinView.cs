using System;
using System.Threading;
using Core.AssetProvider;
using Core.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.UI.Components
{
    public class UIEntitySkinViewInstaller : Installer<UIEntitySkinViewInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<UIEntitySkinView.Factory>()
                .AsSingle();
        }
    }
    public class UIEntitySkinView: MonoBehaviour
    {
        [SerializeField] private RawImage renderedImage;
        private Action _onDestroyCallback;

        public void Initialize(RenderTexture renderTexture)
        {
            renderedImage.texture = renderTexture;
        }

        public void AddOnDestroyCallback(Action callback)
        {
            _onDestroyCallback = callback;
        }

        public void OnDestroy()
        {
            _onDestroyCallback?.Invoke();
        }

        public class AssetProvider : AssetProvider<UIEntitySkinView>
        {
            public const string ASSET_PATH = "UIEntitySkinView";
        }

        public class Factory : IDisposable
        {
            private AssetsProvider<BaseEntityView, int> _tileAssetsProvider;
            private AssetProvider _viewAssetProvider;
            private SkinPreviewCamera.Factory _cameraFactory;

            public Factory(SkinPreviewCamera.Factory cameraFactory)
            {
                _cameraFactory = cameraFactory;
                _tileAssetsProvider = new EntitySingleAssetsProvider();
                _viewAssetProvider = new AssetProvider();
            }
            public async UniTask<(UIEntitySkinView skinView, SkinPreviewCamera camera)> Create(Material material, RectTransform parent, CancellationToken ct)
            {
                var view = GameObject.Instantiate(_viewAssetProvider.GetAsset(), parent);
                try
                {
                    return await BindExisting(view, material, ct);
                }
                catch(OperationCanceledException)
                {
                    GameObject.Destroy(view);
                }
                return default;
            }

            public async UniTask<(UIEntitySkinView skinView, SkinPreviewCamera camera)> BindExisting(UIEntitySkinView view, Material material, CancellationToken ct)
            {
                try
                {
                    await _tileAssetsProvider.LoadAssets(ct, "base");
                    await _viewAssetProvider.LoadAsset(ct, AssetProvider.ASSET_PATH);
                    var tilePrefab = _tileAssetsProvider.GetAsset(1);
                    var skinPreviewCamera = await _cameraFactory.Create(tilePrefab, material, ct);
                    view.Initialize(skinPreviewCamera.RenderTexture);
                    view.AddOnDestroyCallback(()=>Destroy(skinPreviewCamera.gameObject));
                    return (view,skinPreviewCamera);
                }
                catch(OperationCanceledException)
                {
                    
                }
                return default;
            }

            public void Dispose()
            {
                _tileAssetsProvider?.Dispose();
                _viewAssetProvider?.Dispose();
            }
        }
    }
}