using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.AssetProvider;
using Core.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Common
{
    public interface ISkinMaterialMapProvider
    {
        public Dictionary<MaterialId, string> DefaultMap { get; }
    }
    internal class SkinMaterialMapProvider : ISkinMaterialMapProvider
    {
        public Dictionary<MaterialId, string> DefaultMap { get; } = new Dictionary<MaterialId, string>()
        {
            { MaterialId.Default,     "URP_Tile_Player"   },
            { MaterialId.Opponent, "URP_Tile_Opponent" },
            { MaterialId.GoldenYellow, "URP_Tile_GoldenYellow" },
            { MaterialId.WhiteMint, "URP_Tile_WhiteMint" },
            
        };
    }
    public class SkinMaterialAssetsProvider : AssetsProvider<Material, MaterialId>
    {
        private ISkinMaterialMapProvider _skinMaterialMapProvider;
        private static readonly string[] BaseLabels = { "material" };
        private static Dictionary<MaterialId, string> _map;
        
        public SkinMaterialAssetsProvider(ISkinMaterialMapProvider skinMaterialMapProvider)
            : base(KeyFromMaterialName)
        {
            _skinMaterialMapProvider = skinMaterialMapProvider;
        }

        public async UniTask LoadAll(CancellationToken ct, params string[] extraLabels)
        {
            //need user preferences preloaded for proper decoration
            _map = _skinMaterialMapProvider.DefaultMap;
            
            var labels = (extraLabels == null || extraLabels.Length == 0)
                ? BaseLabels
                : BaseLabels.Concat(extraLabels).ToArray();

            await LoadAssetsByLabels(ct, Addressables.MergeMode.Intersection, labels);
        }

        public Material Get(MaterialId id, bool instantiate = false)
        {
            var m = GetAsset(id);
            return instantiate ? UnityEngine.Object.Instantiate(m) : m;
        }

        private static MaterialId KeyFromMaterialName(Material mat)
        {
            if (!mat) return MaterialId.Unknown;

            foreach (var kv in _map)
            {
                var needle = kv.Value;
                if (!string.IsNullOrEmpty(needle) &&
                    mat.name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kv.Key;
            }

            return MaterialId.Unknown;
        }
    }
}