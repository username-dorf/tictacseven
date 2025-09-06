using System.Threading;
using Core.Common;
using Cysharp.Threading.Tasks;

namespace Core.AssetProvider
{
    public class EntitySingleAssetsProvider : AssetsProvider<BaseEntityView, int>
    {
        private const string ASSET_PATH = "Entity_Base_tiled";

        public EntitySingleAssetsProvider()
            : base(SelectKey, ResolveKey, true)
        {
        }

        public override UniTask LoadAssets(CancellationToken ct, params string[] assetKeys)
        {
            return base.LoadAssets(ct, ASSET_PATH);
        }

        public static int SelectKey(IMaterialApplicableView gameObject)
        {
            return 1;
        }

        public static int ResolveKey(int key)
        {
            return 1;
        }
    }
}