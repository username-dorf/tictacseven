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
}