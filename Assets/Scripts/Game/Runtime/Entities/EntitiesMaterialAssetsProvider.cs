using Core.Common;

namespace Game.Entities
{
    public sealed class EntitiesMaterialAssetsProvider : SkinMaterialAssetsProvider
    {
        public EntitiesMaterialAssetsProvider(ISkinMaterialMapProvider skinMaterialMapProvider) : base(skinMaterialMapProvider)
        {
        }
    }
}