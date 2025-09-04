using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.UI.Windows
{
    public interface IWindowsController
    {
        IWindow Top { get; }

        UniTask<TView> OpenAsync<TView>(CancellationToken ct = default)
            where TView : WindowViewBase;

        UniTask<TView> OpenAsync<TView, TPayload>(TPayload payload, CancellationToken ct = default)
            where TView : WindowViewBase, IPayloadedWindow<TPayload>;

        UniTask CloseTopAsync(CancellationToken ct = default);
        UniTask CloseAllAsync(CancellationToken ct = default);
    }

    public sealed class WindowsController : IWindowsController, System.IDisposable
    {
        public IWindow Top => _stack.Count > 0 ? _stack.Peek() : null;

        private readonly DiContainer _container;
        private readonly IUIRootService _uiRoot;
        private readonly WindowsAssetsProvider _provider;

        private readonly Stack<IWindow> _stack = new();
        private readonly SemaphoreSlim _mutex = new(1, 1);
        private readonly IUIBlurService _blurService;
        private readonly SceneContextRegistry _sceneRegistry;

        public WindowsController(
            DiContainer container,
            SceneContextRegistry sceneRegistry,
            IUIRootService uiRootService,
            IUIBlurService blurService,
            WindowsAssetsProvider provider)
        {
            _sceneRegistry = sceneRegistry;
            _blurService = blurService;
            _container = container;
            _uiRoot = uiRootService;
            _provider = provider;
        }

        public async UniTask<TView> OpenAsync<TView>(CancellationToken ct = default)
            where TView : WindowViewBase
        {
            await _mutex.WaitAsync(ct);
            try
            {
                PruneDead();

                var hadWindows = _stack.Count > 0;
                var prev = Top;
                var prevView = prev as WindowViewBase;

                if (IsAlive(prev) && prevView != null && !prevView.IsClosing)
                {
                    try
                    {
                        await prevView.HideAsync(ct);
                    }
                    catch
                    {
                         /* ignore */
                    }
                }

                if (!hadWindows)
                    _blurService.Blur.gameObject.SetActive(true);

                var parent = await _uiRoot.WaitForRootAsync(ct);
                var sceneContainer = GetSceneContainerFor(parent) ?? _container;
                var prefabComp = (TView)_provider.GetAsset(typeof(TView));
                var instance = sceneContainer.InstantiatePrefabForComponent<TView>(prefabComp.gameObject, parent);

                await instance.OpenAsync(ct);
                _stack.Push(instance);
                return instance;
            }
            finally { _mutex.Release(); }
        }

        public async UniTask<TView> OpenAsync<TView, TPayload>(TPayload payload, CancellationToken ct = default)
            where TView : WindowViewBase, IPayloadedWindow<TPayload>
        {
            await _mutex.WaitAsync(ct);
            try
            {
                PruneDead();

                var hadWindows = _stack.Count > 0;
                var prev = Top;
                var prevView = prev as WindowViewBase;

                if (IsAlive(prev) && prevView != null && !prevView.IsClosing)
                {
                    try { await prevView.HideAsync(ct); } catch { /* ignore */ }
                }

                if (!hadWindows)
                    _blurService.Blur.gameObject.SetActive(true);

                var parent = await _uiRoot.WaitForRootAsync(ct);
                var sceneContainer = GetSceneContainerFor(parent) ?? _container;
                var prefabComp = (TView)_provider.GetAsset(typeof(TView));
                var instance = sceneContainer.InstantiatePrefabForComponent<TView>(prefabComp.gameObject, parent);

                instance.SetPayload(payload);
                await instance.OpenAsync(ct);

                _stack.Push(instance);
                return instance;
            }
            finally { _mutex.Release(); }
        }

        public async UniTask CloseTopAsync(CancellationToken ct = default)
        {
            await _mutex.WaitAsync(ct);
            try
            {
                PruneDead();
                if (_stack.Count == 0)
                {
                    _blurService.Blur.gameObject.SetActive(false);
                    return;
                }

                var top = _stack.Pop() as WindowViewBase;
                if (top != null && IsAlive(top))
                {
                    try { await top.CloseAsync(ct); } catch { /* ignore */ }
                }

                PruneDead();
                var next = Top as WindowViewBase;

                if (IsAlive(next) && next != null && !next.IsClosing)
                {
                    try { await next.ShowAsync(ct); } catch { /* ignore */ }
                }

                if (_stack.Count == 0)
                    _blurService.Blur.gameObject.SetActive(false);
            }
            finally { _mutex.Release(); }
        }

        public async UniTask CloseAllAsync(CancellationToken ct = default)
        {
            await _mutex.WaitAsync(ct);
            try
            {
                while (_stack.Count > 0)
                {
                    var w = _stack.Pop() as WindowViewBase;
                    if (IsAlive(w) && w != null)
                    {
                        try { await w.CloseAsync(ct); } catch { /* ignore */ }
                    }
                }
                _blurService.Blur.gameObject.SetActive(false);
            }
            finally { _mutex.Release(); }
        }

        public void Dispose()
        {
            _mutex?.Dispose();
        }

        private DiContainer GetSceneContainerFor(Transform parent)
        {
            var scene = parent.gameObject.scene;
            return _sceneRegistry.GetContainerForScene(scene);
        }

        private static bool IsAlive(object w)
        {
            if (w is null) return false;
            var uo = w as UnityEngine.Object;
            return uo != null;
        }

        private void PruneDead()
        {
            while (_stack.Count > 0)
            {
                var top = _stack.Peek();
                if (IsAlive(top)) break;
                _stack.Pop();
            }
        }
    }
}
