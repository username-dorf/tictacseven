using System;
using System.Threading;
using Core.Common;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Core.Audio.Players
{
    public interface IMusicPlayer
    {
        UniTask SetMusic(MusicKey key, float fadeOut = 0.3f, float fadeIn = 0.6f, bool loop = true);
    }

    public sealed class MusicPlayer : IMusicPlayer, IInitializable, IDisposable
    {
        private readonly MusicClipsProvider _clips;
        private readonly IAppPreferencesProvider _appPreferences;
        private readonly CompositeDisposable _disposables;
        private readonly Transform _root;

        private AudioSource _source1;
        private AudioSource _source2;
        private bool _source1Active = true;
        private CancellationTokenSource _fadeCts;

        private (MusicKey key, float fadeOut, float fadeIn, bool loop) _lastRequest =
            (MusicKey.None, 0.3f, 0.6f, true);

        private int _savedTimeSamples = 0;

        public MusicPlayer(
            MusicClipsProvider clips,
            IAppPreferencesProvider appPreferences,
            [Inject(Id = "AudioRoot")] Transform root)
        {
            _clips = clips;
            _appPreferences = appPreferences;
            _root = root;
            _disposables = new CompositeDisposable();
        }

        public void Initialize()
        {
            _source1 = CreateSource("Music_A");
            _source2 = CreateSource("Music_B");

            _appPreferences.Current.Music
                .Subscribe(ApplySettings)
                .AddTo(_disposables);

            _appPreferences.Current.Music
                .Subscribe(OnMusicEnabledChanged)
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
        }

        private async void OnMusicEnabledChanged(bool enabled)
        {
            var active = _source1Active ? _source1 : _source2;

            if (enabled)
            {
                if (active.clip != null && !active.isPlaying)
                {
                    if (_savedTimeSamples > 0 && _savedTimeSamples < active.clip.samples)
                        active.timeSamples = _savedTimeSamples;

                    active.Play();
                    await Fade(active, 0f, 1, 0.25f, default);
                }
                else if (_lastRequest.key != MusicKey.None)
                {
                    await SetMusic(_lastRequest.key, 0f, _lastRequest.fadeIn, _lastRequest.loop);
                }
            }
            else
            {
                if (active.isPlaying)
                {
                    _savedTimeSamples = active.timeSamples;
                    await Fade(active, active.volume, 0f, 0.2f, default);
                    active.Stop();
                }
            }
        }

        private AudioSource CreateSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            return src;
        }

        private void ApplySettings(bool enabled)
        {
            _source1.mute = !enabled;
            _source2.mute = !enabled;
        }

        public async UniTask SetMusic(MusicKey key, float fadeOut = 0.3f, float fadeIn = 0.6f, bool loop = true)
        {
            _lastRequest = (key, fadeOut, fadeIn, loop);
            _fadeCts?.Cancel();
            _fadeCts = new CancellationTokenSource();

            if (key == MusicKey.None)
            {
                await CrossFade(null, fadeOut, fadeIn, loop, _fadeCts.Token);
                return;
            }

            if (!_clips.TryGetAsset(key, out var clip))
                return;

            await CrossFade(clip, fadeOut, fadeIn, loop, _fadeCts.Token);
        }

        private async UniTask CrossFade(AudioClip target, float fadeOut, float fadeIn, bool loop, CancellationToken ct)
        {
            var from = _source1Active ? _source1 : _source2;
            var to = _source1Active ? _source2 : _source1;

            if (target == null)
            {
                await Fade(from, from.volume, 0f, fadeOut, ct);
                from.Stop();
                return;
            }

            to.clip = target;
            to.loop = loop;
            to.volume = 0f;

            if (_appPreferences.Current.Music.Value)
            {
                to.Play();
                float baseVol = 1;
                await UniTask.WhenAll(
                    Fade(from, from.volume, 0f, fadeOut, ct),
                    Fade(to, 0f, baseVol, fadeIn, ct)
                );
                from.Stop();
                _source1Active = !_source1Active;
            }
            else
            {
                to.volume = 0f;
                from.Stop();
                _source1Active = !_source1Active;
            }
        }

        private async UniTask Fade(AudioSource src, float from, float to, float time, CancellationToken ct)
        {
            if (time <= 0f)
            {
                src.volume = to;
                return;
            }

            float t = 0f;
            while (t < time && !ct.IsCancellationRequested)
            {
                t += Time.unscaledDeltaTime;
                src.volume = Mathf.Lerp(from, to, t / time);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            src.volume = to;
        }
    }
}