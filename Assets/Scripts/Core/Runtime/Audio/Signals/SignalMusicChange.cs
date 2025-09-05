namespace Core.Audio.Signals
{
    public readonly struct SignalMusicChange
    {
        public readonly MusicKey Key;
        public readonly float FadeOut;
        public readonly float FadeIn;
        public readonly bool Loop;

        public SignalMusicChange(MusicKey key, float fadeOut = 0.3f, float fadeIn = 0.6f, bool loop = true)
        {
            Key = key;
            FadeOut = fadeOut;
            FadeIn = fadeIn;
            Loop = loop;
        }
    }
}