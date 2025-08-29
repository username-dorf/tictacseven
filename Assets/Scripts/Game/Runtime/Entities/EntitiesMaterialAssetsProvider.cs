using System.Collections.Generic;
using Core.Common;
using Core.Data;
using Core.User;
using Zenject;

namespace Game.Entities
{
    public class GameSkinMaterialMapProvider : ISkinMaterialMapProvider
    {
        private ISkinMaterialMapProvider _skinMaterialMapProvider;
        private IUserPreferencesProvider _userPreferencesProvider;

        public Dictionary<MaterialId, string> DefaultMap
        {
            get
            {
                var userTileMaterialId = _userPreferencesProvider.Current.TileMaterialId.Value;
                var originMap = _skinMaterialMapProvider.DefaultMap;
                var opponentTileMaterial = originMap[MaterialId.Opponent];
                var userTileMaterial = originMap[userTileMaterialId];
                return new Dictionary<MaterialId, string>()
                {
                    {MaterialId.Opponent, opponentTileMaterial},
                    {MaterialId.Default, userTileMaterial}
                };
            }
        }

        public GameSkinMaterialMapProvider(
            ISkinMaterialMapProvider skinMaterialMapProvider,
            IUserPreferencesProvider userPreferencesProvider)
        {
            _userPreferencesProvider = userPreferencesProvider;
            _skinMaterialMapProvider = skinMaterialMapProvider;
        }

        
    }

    public sealed class EntitiesMaterialAssetsProvider : SkinMaterialAssetsProvider
    {
        public EntitiesMaterialAssetsProvider(ISkinMaterialMapProvider skinMaterialMapProvider) : base(skinMaterialMapProvider)
        {
        }
    }
}