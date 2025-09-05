using Core.Audio.Players;
using Core.Audio.Signals;
using Zenject;

namespace Core.Audio
{
    public sealed class AudioSignalHandler : IInitializable, System.IDisposable
    {
        private readonly SignalBus _bus;
        private readonly ISfxPlayer _sfx;
        private readonly IMusicPlayer _music;

        public AudioSignalHandler(SignalBus bus, ISfxPlayer sfx, IMusicPlayer music)
        {
            _bus = bus;
            _sfx = sfx;
            _music = music;
        }

        public void Initialize()
        {
            _bus.Subscribe<SignalSfxPlay>(OnPlaySfx);
            _bus.Subscribe<SignalMusicChange>(OnChangeMusic);
        }

        public void Dispose()
        {
            _bus.TryUnsubscribe<SignalSfxPlay>(OnPlaySfx);
            _bus.TryUnsubscribe<SignalMusicChange>(OnChangeMusic);
        }

        private void OnPlaySfx(SignalSfxPlay s)
        {
            _sfx.Play(s.Key, s.WorldPos, s.VolumeScale, s.Pitch);
        }

        private async void OnChangeMusic(SignalMusicChange s)
        {
            await _music.SetMusic(s.Key, s.FadeOut, s.FadeIn, s.Loop);
        }
    }
}