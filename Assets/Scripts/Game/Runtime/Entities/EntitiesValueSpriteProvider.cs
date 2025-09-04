using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Core.AssetProvider;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Entities
{
    public sealed class EntitiesValueSpriteProvider : AssetsProvider<Sprite, int>
    {
        private static readonly string[] BaseLabels = { "digit", "sprite" };

        public EntitiesValueSpriteProvider()
            : base(SelectKeyFromName) { }

        public async UniTask LoadAll(CancellationToken ct, params string[] labels)
        {
            var all = (labels == null || labels.Length == 0)
                ? BaseLabels
                : BaseLabels.Concat(labels).ToArray();

            await LoadAssetsByLabels(ct, Addressables.MergeMode.Intersection, all);
        }

        private static int SelectKeyFromName(Sprite s)
        {
            var name = s ? s.name : null;
            if (string.IsNullOrEmpty(name)) return 0;
            var m = Regex.Match(name, @"\d+");
            return m.Success && int.TryParse(m.Value, out var id) ? id : 0;
        }
    }
}