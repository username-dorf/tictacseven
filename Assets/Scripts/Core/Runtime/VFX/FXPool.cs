using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Toolkit;
using UnityEngine;
using Zenject;

namespace Core.VFX
{
    public interface IFxPool
    {
        void Warmup(int count);
        void Play(Vector3 worldPos, Vector3? upNormal, float scale = 1f, Color? tint = null);
        int ActiveCount { get; }
    }
    public abstract class FxPoolService : ObjectPool<PooledFXView>, IFxPool
    {
        private readonly PooledFXView _prefab;
        private readonly float _autoRecycleAfter;
        private readonly int _prewarm;
        private readonly int _hardCap;

        private readonly Dictionary<PooledFXView, SerialDisposable> _life = new();
        private readonly LinkedList<PooledFXView> _activeOrder = new();

        private Transform _parent;
        
        public int ActiveCount => _activeOrder.Count;
        
        [Inject]
        public FxPoolService(
            [Inject(Id = "FX_PlacePiece_Prefab")] PooledFXView prefab,
            [InjectOptional(Id = "FX_Place_AutoRecycle")] float autoRecycleAfter = 1.6f,
            [InjectOptional(Id = "FX_Place_Prewarm")] int prewarm = 16,
            [InjectOptional(Id = "FX_Place_HardCap")] int hardCap = 0
        )
        {
            _prefab = prefab;
            _autoRecycleAfter = Mathf.Max(0.05f, autoRecycleAfter);
            _prewarm = Mathf.Max(0, prewarm);
            _hardCap = Mathf.Max(0, hardCap);
        }

        protected override PooledFXView CreateInstance()
        {
            var go = UnityEngine.Object.Instantiate(_prefab, _parent);
            go.gameObject.SetActive(false);
            return go;
        }

        private void SafeReturn(PooledFXView fx)
        {
            if (fx == null) 
                return;

            if (_life.TryGetValue(fx, out var sd))
            {
                sd.Disposable = Disposable.Empty;
                _life.Remove(fx);
            }

            fx.StopImmediate();
            Return(fx);
            
            var node = _activeOrder.Find(fx);
            if (node != null) 
                _activeOrder.Remove(node);
        }

        public void Initialize(Transform parent)
        {
            _parent = parent;
            Warmup(_prewarm);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var kv in _life)
            {
                kv.Value.Disposable = Disposable.Empty;
            }
            _life.Clear();
            _activeOrder.Clear();
            base.Dispose(disposing);
        }
        
        public void Warmup(int count)
        {
            if (count <= 0) 
                return;
            var tmp = new List<PooledFXView>(count);

            for (int i = 0; i < count; i++)
            {
                tmp.Add(Rent());
            }

            for (int i = 0; i < count; i++)
            {
                Return(tmp[i]);
            }
        }

        public void Play(Vector3 worldPos, Vector3? upNormal, float scale = 1f, Color? tint = null)
        {
            PooledFXView fx;

            if (_hardCap > 0 && _activeOrder.Count >= _hardCap)
            {
                fx = _activeOrder.First.Value;
                _activeOrder.RemoveFirst();

                if (_life.TryGetValue(fx, out var sdOld))
                {
                    sdOld.Disposable = Disposable.Empty;
                    _life.Remove(fx);
                }

                fx.StopImmediate();
            }
            else
            {
                fx = Rent();
            }

            fx.PlayAt(worldPos, upNormal, scale, tint);
            var node = _activeOrder.AddLast(fx);

            var sd = new SerialDisposable();
            sd.Disposable = Observable
                .Timer(TimeSpan.FromSeconds(_autoRecycleAfter))
                .Subscribe(_ =>
                {
                    SafeReturn(fx);
                });

            _life[fx] = sd;

            fx.gameObject.AddComponent<OnDestroyHook>().Init(() =>
            {
                if (_life.TryGetValue(fx, out var sdKill))
                {
                    sdKill.Disposable = Disposable.Empty;
                    _life.Remove(fx);
                }
            });
        }

        private sealed class OnDestroyHook : MonoBehaviour
        {
            private Action _onDestroy;
            public void Init(Action onDestroy)
            {
                _onDestroy = onDestroy;
            }
            private void OnDestroy()
            {
                _onDestroy?.Invoke();
            }
        }
    }
}