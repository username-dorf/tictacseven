using System;
using System.Collections.Generic;
using Core.AssetProvider;
using UnityEngine;

namespace Core.Audio
{
    public class SfxClipsProvider : AssetsProvider<AudioClip, SfxKey>
    {
        public SfxClipsProvider()
            : base(KeySelector)
        {
        }

        private static readonly Dictionary<string, SfxKey> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ui_click"] = SfxKey.Ui_Click,
            ["ui_pop"] = SfxKey.Ui_Pop,
        };

        private static SfxKey KeySelector(AudioClip clip)
        {
            var name = clip.name.Replace(' ', '_');
            if (_map.TryGetValue(name, out var key))
                return key;

            throw new KeyNotFoundException($"Missing SfxKey for clip '{clip.name}");
        }
    }
}