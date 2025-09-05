using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Audio
{
    public class AudioAssetsPreloader: IDisposable
    {
        private readonly SfxClipsProvider _sfx;
        private readonly MusicClipsProvider _music;
        private readonly string[] _sfxLabels;
        private readonly string[] _musicLabels;
        private readonly Addressables.MergeMode _mergeMode;

        public AudioAssetsPreloader(
            SfxClipsProvider sfx,
            MusicClipsProvider music,
            string[] sfxLabels,
            string[] musicLabels,
            Addressables.MergeMode mergeMode)
        {
            _sfx = sfx;
            _music = music;
            _sfxLabels = sfxLabels;
            _musicLabels = musicLabels;
            _mergeMode = mergeMode;
        }

        public async UniTask LoadAssetsAsync(CancellationToken ct)
        {
            try
            {
                await _sfx.LoadAssetsByLabels(ct, _mergeMode, _sfxLabels);
                await _music.LoadAssetsByLabels(ct, _mergeMode, _musicLabels);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Dispose()
        {
            _sfx.ReleaseAll();
            _music.ReleaseAll();
        }
    }
}