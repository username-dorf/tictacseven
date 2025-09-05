using UnityEngine;

namespace Core.Audio.Signals
{
    public readonly struct SignalSfxPlay
    {
        public readonly SfxKey Key;
        public readonly Vector3? WorldPos; // null => 2D (UI)
        public readonly float VolumeScale;
        public readonly float Pitch;

        public SignalSfxPlay(SfxKey key, Vector3? worldPos = null, float volumeScale = 1f, float pitch = 1f)
        {
            Key = key;
            WorldPos = worldPos;
            VolumeScale = volumeScale;
            Pitch = pitch;
        }
    }
}