using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.AssetProvider
{
    public interface IAssetLoader
    {
        bool IsLoaded { get; }
        UniTask LoadAsset(CancellationToken cancellationToken, string assetKey);
    }
    public interface IAssetsLoader : IDisposable
    {
        bool IsLoaded { get; }
        UniTask LoadAssets(CancellationToken cancellationToken, params string[] assetKeys);
    }
}