using Core.Audio.Signals;
using Zenject;

namespace Core.Audio
{
    public class SceneMusicBootstrap
    {
        private SignalBus _signalBus;
        private bool _launched;

        public SceneMusicBootstrap(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        public void Initialize()
        {
            if(_launched)
                return;
            _launched = true;
            _signalBus.Fire(new SignalMusicChange(MusicKey.Ambient));
        }
    }
}