using System;
using System.Collections.Generic;
using Core.Common;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Core.Audio.Players
{
    public interface ISfxPlayer
    {
        void Play(SfxKey key, Vector3? worldPos = null, float volumeScale = 1f, float pitch = 1f);
    }
    public sealed class SfxPlayer : ISfxPlayer, IInitializable, IDisposable
    {
        private readonly SfxClipsProvider _clips;
        private readonly CompositeDisposable _disposables;
        private readonly Transform _root;
        private AudioSource _uiSource;

        private readonly Queue<AudioSource> _pool;
        private readonly List<AudioSource> _activeSources;
        private readonly int _poolSize;

        private readonly Dictionary<SfxKey, double> _cooldownUntil;
        private readonly TimeSpan _defaultCooldown = TimeSpan.FromMilliseconds(40);
        private readonly IAppPreferencesProvider _appPreferences;

        public SfxPlayer(
            SfxClipsProvider clips,
            IAppPreferencesProvider appPreferences,
            [Inject(Id = "AudioRoot")] Transform root,
            [Inject(Id = "Sfx3DPoolSize")] int poolSize = 3)
        {
            _appPreferences = appPreferences;
            _clips = clips;
            _root = root;
            _poolSize = Mathf.Max(1, poolSize);
            
            _pool = new Queue<AudioSource>(_poolSize);
            _activeSources = new List<AudioSource>();
            _cooldownUntil = new Dictionary<SfxKey, double>();

            _disposables = new CompositeDisposable();
        }

        public void Initialize()
        {
            var uiGo = new GameObject("SFX_UI_Source");
            uiGo.transform.SetParent(_root, false);
            _uiSource = uiGo.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;
            _uiSource.loop = false;
            _uiSource.spatialBlend = 0f;

            PrewarmPool();

            _appPreferences.Current.Sound
                .Subscribe(ApplySettings)
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void ApplySettings(bool enabled)
        {
            _uiSource.mute = !enabled;
            foreach (var a in _pool)
            {
                a.mute = !enabled;
            }

            foreach (var a in _activeSources)
            {
                a.mute = !enabled;
            }
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var go = new GameObject($"SFX_3D_{i}");
                go.transform.SetParent(_root, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.spatialBlend = 1f; // 3D
                src.rolloffMode = AudioRolloffMode.Linear;
                _pool.Enqueue(src);
            }
        }

        public void Play(SfxKey key, Vector3? worldPos = null, float volumeScale = 1f, float pitch = 1f)
        {
            if (!_appPreferences.Current.Sound.Value) 
                return;

            var now = Time.realtimeSinceStartupAsDouble;
            if (_cooldownUntil.TryGetValue(key, out var until) && now < until) 
                return;
            _cooldownUntil[key] = now + _defaultCooldown.TotalSeconds;

            if (!_clips.TryGetAsset(key, out var clip)) 
                return;

            if (worldPos == null)
            {
                // 2D/UI
                _uiSource.pitch = pitch;
                _uiSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
            }
            else
            {
                // 3D
                var src = RentSource();
                src.transform.position = worldPos.Value;
                src.pitch = pitch;
                src.clip = clip;
                src.volume = Mathf.Clamp01(volumeScale);
                src.Play();
                ReturnAfter(src, clip.length).Forget();
            }
        }

        private AudioSource RentSource()
        {
            var src = _pool.Count > 0 ? _pool.Dequeue() : CreateOverflowSource();
            src.gameObject.SetActive(true);
            _activeSources.Add(src);
            return src;
        }

        private AudioSource CreateOverflowSource()
        {
            var go = new GameObject($"SFX_3D_Overflow");
            go.transform.SetParent(_root, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            return src;
        }

        private async UniTaskVoid ReturnAfter(AudioSource src, float sec)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(sec + 0.05f));
            src.clip = null;
            src.gameObject.SetActive(false);
            _activeSources.Remove(src);
            _pool.Enqueue(src);
        } 
    }
}