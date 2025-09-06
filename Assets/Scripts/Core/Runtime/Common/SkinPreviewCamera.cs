using System;
using System.Threading;
using Core.AssetProvider;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Common
{
    public class SkinPreviewCamera : MonoBehaviour
    {
        [SerializeField] private Camera camera;
        public IMaterialApplicableView PreviewView { get; private set; }
        public RenderTexture RenderTexture { get; private set; }
        
        public void Initialize(IMaterialApplicableView previewView)
        {
            this.PreviewView = previewView;
            RenderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            camera.targetTexture = RenderTexture;
            camera.enabled = false;
            camera.Render();
        }

        public void UpdateRender()
        {
            camera.Render();
        }
        
        public void OnDestroy()
        {
            camera.targetTexture = null;
            if (RenderTexture != null)
            {
                RenderTexture.Release();
                Destroy(RenderTexture);
            }
        }
        public class AssetProvider : AssetProvider<SkinPreviewCamera>
        {
            public const string ASSET_PATH = "SkinPreviewCamera";
        }
        
        public class Factory : IDisposable
        {
            private readonly Vector3 TILE_DEFAULT_ROTATION = new Vector3(-29.5f, -55.5f, 35.5f);
            private readonly Vector3 TILE_DEFAULT_POSITION = new Vector3(0, -0.5f, 3);

            private AssetProvider _assetProvider;
            private CancellationTokenSource _cancellationTokenSource;
            
            public Factory()
            {
                _assetProvider = new AssetProvider();
            }
            
            public async UniTask<SkinPreviewCamera> Create(BaseEntityView tilePrefab, Material skinMaterial, CancellationToken ct)
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);

                try
                {
                    await _assetProvider.LoadAsset(ct, AssetProvider.ASSET_PATH);
                    var camera = GameObject.Instantiate(_assetProvider.GetAsset());
                    var tile = GameObject.Instantiate(tilePrefab, camera.transform);
                    tile.transform.localEulerAngles = TILE_DEFAULT_ROTATION;
                    tile.transform.localPosition = TILE_DEFAULT_POSITION;
                    tile.gameObject.SetLayerRecursively(LayerMask.NameToLayer("SkinPreviewOnly"));
                    tile.ChangeMaterial(skinMaterial);
                    camera.Initialize(tile);
                    return camera;
                }
                catch (OperationCanceledException)
                {
                    
                }

                return null;
            }

            public void Dispose()
            {
                _assetProvider?.Dispose();
                
                if(_cancellationTokenSource is not null && !_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
        }
    }
}