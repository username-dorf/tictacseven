using System;
using System.Collections.Generic;
using Core.AssetProvider;
using UnityEngine;

namespace Core.Audio
{
    public class MusicClipsProvider : AssetsProvider<AudioClip, MusicKey>
    {
        public MusicClipsProvider()
            : base(KeySelector)
        {
        }

        private static readonly Dictionary<string, MusicKey> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            ["music_ambient"] = MusicKey.Ambient,
        };

        private static MusicKey KeySelector(AudioClip clip)
        {
            var name = clip.name.Replace(' ', '_');
            if (_map.TryGetValue(name, out var key))
                return key;

            throw new KeyNotFoundException($"Missing MusicKey for clip '{clip.name}'.");
        }
    }
}