using UnityEngine;

namespace Core.VFX
{
    public class PooledFXView : MonoBehaviour
    {
        private ParticleSystem[] _systems;
        private ParticleSystemRenderer[] _renderers;
        private Material[] _baseMaterials;

        private void Awake()
        {
            _systems = GetComponentsInChildren<ParticleSystem>(true);
            _renderers = new ParticleSystemRenderer[_systems.Length];
            _baseMaterials = new Material[_systems.Length];

            for (int i = 0; i < _systems.Length; i++)
            {
                var renderer = _systems[i].GetComponent<ParticleSystemRenderer>();
                _renderers[i] = renderer;

                if (renderer != null && renderer.material != null)
                {
                    _baseMaterials[i] = renderer.material;
                }
            }
        }

        public void PlayAt(Vector3 worldPos, Vector3? upNormal, float uniformScale, Material customMaterial = null)
        {
            transform.position = worldPos;

            if (upNormal.HasValue && upNormal.Value.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.FromToRotation(Vector3.up, upNormal.Value.normalized);

            transform.localScale = Vector3.one * Mathf.Max(0.0001f, uniformScale);

            for (int i = 0; i < _systems.Length; i++)
            {
                var ps = _systems[i];
                var renderer = _renderers[i];

                ps.Clear(true);

                if (customMaterial != null && renderer != null)
                {
                    renderer.material = customMaterial;
                }
                else if (renderer != null && _baseMaterials[i] != null)
                {
                    renderer.material = _baseMaterials[i];
                }

                ps.Play(true);
            }

            gameObject.SetActive(true);
        }

        public void StopImmediate()
        {
            foreach (var ps in _systems)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _baseMaterials[i] != null)
                {
                    _renderers[i].material = _baseMaterials[i];
                }
            }

            gameObject.SetActive(false);
        }
    }
}