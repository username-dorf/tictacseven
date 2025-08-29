using System.Collections.ObjectModel;

namespace Core.AssetProvider
{
    public interface IAssetProvider
    {
            
    }
    public interface IAssetProvider<out T>: IAssetProvider
    {
        T GetAsset();
    }

    public interface IAssetsProvider<out T, in TKey>: IAssetProvider
    {
        T GetAsset(TKey key);
    }

    public interface IAssetReader<T, TKey>
    {
        ReadOnlyDictionary<TKey, T> GetAllAssets();
    }
}