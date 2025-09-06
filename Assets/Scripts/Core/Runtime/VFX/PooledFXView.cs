using UnityEngine;

namespace Core.VFX
{
    public class PooledFXView : MonoBehaviour
    {
        private ParticleSystem[] _systems;
        private Color[] _baseColors;
        private bool _hasSimpleStartColors;

        private void Awake()
        {
            _systems = GetComponentsInChildren<ParticleSystem>(true);

            _baseColors = new Color[_systems.Length];
            _hasSimpleStartColors = true;
            for (int i = 0; i < _systems.Length; i++)
            {
                var main = _systems[i].main;
                if (main.startColor.mode == ParticleSystemGradientMode.Color)
                    _baseColors[i] = main.startColor.color;
                else
                    _hasSimpleStartColors = false;
            }
        }

        public void PlayAt(Vector3 worldPos, Vector3? upNormal, float uniformScale, Color? tint)
        {
            transform.position = worldPos;

            if (upNormal.HasValue && upNormal.Value.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.FromToRotation(Vector3.up, upNormal.Value.normalized);

            transform.localScale = Vector3.one * Mathf.Max(0.0001f, uniformScale);

            for (int i = 0; i < _systems.Length; i++)
            {
                var ps = _systems[i];
                ps.Clear(true);

                if (tint.HasValue && _hasSimpleStartColors && i < _baseColors.Length)
                {
                    var main = ps.main;
                    var b = _baseColors[i];
                    main.startColor = new Color(
                        Mathf.Lerp(b.r, tint.Value.r, 0.35f),
                        Mathf.Lerp(b.g, tint.Value.g, 0.35f),
                        Mathf.Lerp(b.b, tint.Value.b, 0.35f),
                        b.a
                    );
                }

                ps.Play(true);
            }

            gameObject.SetActive(true);
        }

        public void StopImmediate()
        {
            foreach (var ps in _systems)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            gameObject.SetActive(false);
        }
    }
}