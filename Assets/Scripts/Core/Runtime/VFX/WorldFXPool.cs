using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Toolkit;
using UnityEngine;
using Zenject;

namespace Core.VFX
{
    public interface IWorldFxPool
    {
        void Warmup(int count);
        void PlayAtWorldPosition(Vector3 worldPos, Vector3? upNormal, float scale = 1f, Material customMaterial = null);

        UniTask PlayAlongPathAsync(
            IReadOnlyList<Vector3> pathWorld,
            Vector3? upNormal,
            float scale = 1f,
            Material customMaterial = null,
            float moveSpeed = 2f,
            bool loopPath = false,
            CancellationToken token = default);
        int ActiveCount { get; }
    }

    public class WorldFxPoolService : ObjectPool<PooledFXView>, IWorldFxPool
    {
        private readonly Dictionary<string, PooledFXView> _prefabs = new();
        private readonly float _autoRecycleAfter;
        private readonly int _prewarm;
        private readonly int _hardCap;

        private readonly Dictionary<PooledFXView, SerialDisposable> _life = new();
        private readonly LinkedList<PooledFXView> _activeOrder = new();

        private Transform _worldParent;
        private bool _initialized = false;

        public int ActiveCount => _activeOrder.Count;

        [Inject]
        public WorldFxPoolService(
            [InjectOptional(Id = "WorldFX_AutoRecycle")]
            float autoRecycleAfter,
            [InjectOptional(Id = "WorldFX_Prewarm")]
            int prewarm,
            [InjectOptional(Id = "WorldFX_HardCap")]
            int hardCap
        )
        {
            _autoRecycleAfter = Mathf.Max(0.05f, autoRecycleAfter);
            _prewarm = Mathf.Max(0, prewarm);
            _hardCap = Mathf.Max(0, hardCap);
        }

        public void Initialize()
        {
            if (_initialized) return;
            var worldFxGO = new GameObject("WorldFX_Pool");
            _worldParent = worldFxGO.transform;
            UnityEngine.Object.DontDestroyOnLoad(worldFxGO);

            _initialized = true;
            Warmup(_prewarm);
        }

        public void RegisterPrefab(string fxType, PooledFXView prefab)
        {
            if (!_prefabs.ContainsKey(fxType))
            {
                _prefabs[fxType] = prefab;
            }
        }

        protected override PooledFXView CreateInstance()
        {
            var prefab = GetDefaultPrefab();
            if (prefab == null)
            {
                Debug.LogError("No prefabs registered in WorldFxPoolService!");
                return null;
            }

            var go = UnityEngine.Object.Instantiate(prefab, _worldParent);
            go.gameObject.SetActive(false);
            return go;
        }

        private PooledFXView GetDefaultPrefab()
        {
            if (_prefabs.Count == 0) return null;

            foreach (var prefab in _prefabs.Values)
            {
                return prefab;
            }

            return null;
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

        protected override void Dispose(bool disposing)
        {
            foreach (var kv in _life)
            {
                kv.Value.Disposable = Disposable.Empty;
            }

            _life.Clear();
            _activeOrder.Clear();

            if (_worldParent != null)
            {
                UnityEngine.Object.Destroy(_worldParent.gameObject);
            }

            base.Dispose(disposing);
        }

        public void Warmup(int count)
        {
            if (count <= 0 || !_initialized)
                return;

            var tmp = new List<PooledFXView>(count);

            for (int i = 0; i < count; i++)
            {
                var instance = Rent();
                if (instance != null)
                    tmp.Add(instance);
            }

            for (int i = 0; i < tmp.Count; i++)
            {
                Return(tmp[i]);
            }
        }

        public void PlayAtWorldPosition(Vector3 worldPos, Vector3? upNormal, float scale = 1f,
            Material customMaterial = null)
        {
            if (!_initialized)
            {
                Initialize();
            }

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

            if (fx == null) return;

            fx.PlayAt(worldPos, upNormal, scale, customMaterial);
            var node = _activeOrder.AddLast(fx);

            var sd = new SerialDisposable();
            sd.Disposable = Observable
                .Timer(TimeSpan.FromSeconds(_autoRecycleAfter))
                .Subscribe(_ => { SafeReturn(fx); });

            _life[fx] = sd;
        }

        public async UniTask PlayAlongPathAsync(
            IReadOnlyList<Vector3> pathWorld,
            Vector3? upNormal,
            float scale = 1f,
            Material customMaterial = null,
            float moveSpeed = 2f,
            bool loopPath = false,
            CancellationToken token = default)
        {
            if (pathWorld == null || pathWorld.Count < 2)
                return;

            if (!_initialized)
                Initialize();

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

            if (fx == null) return;

            var startPos = pathWorld[0];
            fx.PlayAt(startPos, upNormal, scale, customMaterial);

            var node = _activeOrder.AddLast(fx);
            var sd = new SerialDisposable();
            _life[fx] = sd;

            bool returned = false;

            void ReturnNowSafely()
            {
                if (!returned)
                {
                    returned = true;
                    SafeReturn(fx);
                }
            }

            try
            {
                if (upNormal.HasValue)
                    fx.transform.up = upNormal.Value;

                int seg = 0;
                while (true)
                {
                    for (seg = 0; seg < pathWorld.Count - 1; seg++)
                    {
                        token.ThrowIfCancellationRequested();

                        Vector3 a = pathWorld[seg];
                        Vector3 b = pathWorld[seg + 1];

                        float segLen = Vector3.Distance(a, b);
                        if (segLen < 0.0001f)
                        {
                            fx.transform.position = b;
                            continue;
                        }

                        float segTime = segLen / Mathf.Max(0.0001f, moveSpeed);
                        float t = 0f;

                        while (t < segTime)
                        {
                            token.ThrowIfCancellationRequested();
                            t += Time.deltaTime;
                            float lerp = Mathf.Clamp01(t / segTime);
                            fx.transform.position = Vector3.LerpUnclamped(a, b, lerp);
                            await UniTask.Yield(PlayerLoopTiming.Update, token);
                        }

                        fx.transform.position = b;
                    }

                    if (!loopPath)
                        break;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_autoRecycleAfter), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (_life.TryGetValue(fx, out var alive))
                {
                    alive.Disposable = Disposable.Empty;
                    _life.Remove(fx);
                }

                ReturnNowSafely();
            }
        }
        public async UniTask PlayOnPointAsync(
            Vector3 point,
            Vector3? upNormal,
            float scale = 1f,
            Material customMaterial = null,
            CancellationToken token = default)
        {
            
            if (!_initialized)
                Initialize();

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

            if (fx == null) return;

            var spawnPos = point;
            fx.PlayAt(spawnPos, upNormal, scale, customMaterial);
            var node = _activeOrder.AddLast(fx);
            var sd = new SerialDisposable();
            _life[fx] = sd;

            bool returned = false;
            void ReturnNowSafely()
            {
                if (!returned)
                {
                    returned = true;
                    SafeReturn(fx);
                }
            }

            try
            {
                if (upNormal.HasValue)
                    fx.transform.up = upNormal.Value;

                await UniTask.WaitUntilCanceled(token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (_life.TryGetValue(fx, out var alive))
                {
                    alive.Disposable = Disposable.Empty;
                    _life.Remove(fx);
                }
                ReturnNowSafely();
            }
        }
    }
}