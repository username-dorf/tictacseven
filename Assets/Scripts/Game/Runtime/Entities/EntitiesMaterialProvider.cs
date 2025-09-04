using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.AssetProvider;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Entities
{
    public enum MaterialId
    {
        Unknown = 0,
        User,
        Opponent
    }

    public sealed class EntitiesMaterialProvider : AssetsProvider<Material, MaterialId>
    {
        private static readonly string[] BaseLabels = { "material" };
        private static readonly Dictionary<MaterialId, string> _defaultMap = new Dictionary<MaterialId, string>()
        {
            { MaterialId.User,     "URP_Tile_Player"   },
            { MaterialId.Opponent, "URP_Tile_Opponent" }
        };


        public EntitiesMaterialProvider()
            : base(KeyFromMaterialName)
        {
        }

        public async UniTask LoadAll(CancellationToken ct, params string[] extraLabels)
        {
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

            foreach (var kv in _defaultMap)
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