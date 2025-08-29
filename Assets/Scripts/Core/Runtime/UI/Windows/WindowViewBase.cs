using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UniRx;
using UnityEngine;

namespace Core.UI.Windows
{
    [DisallowMultipleComponent]
    public abstract class WindowViewBase : MonoBehaviour, IWindow
    {
        [SerializeField, Foldout("Base Window")] private CanvasGroup _canvasGroup;
        [SerializeField, Foldout("Base Window")] private bool _useBaseFade = true;
        [SerializeField, Foldout("Base Window")] private float _fadeDuration = 0.12f;

        protected readonly CompositeDisposable Disposables = new();

        protected virtual void Awake()
        {
            if (!_canvasGroup) 
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        public async UniTask OpenAsync(CancellationToken ct)
        {
            gameObject.SetActive(false);
            
            await OnBeforeBindAsync(ct);
            await BindAsync(ct);
            await OnAfterBindAsync(ct);


            if (_canvasGroup)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                if (_useBaseFade) _canvasGroup.alpha = 0f;
                else _canvasGroup.alpha = 1f;
            }
            
            gameObject.SetActive(true);

            if (_canvasGroup && _useBaseFade)
                await UniTask.WhenAll(PlayOpenAnimationAsync(ct), FadeAsync(_canvasGroup.alpha, 1f, _fadeDuration, ct));
            else
                await PlayOpenAnimationAsync(ct);

            if (_canvasGroup)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            await OnAfterOpenAsync(ct);
        }

        public async UniTask CloseAsync(CancellationToken ct)
        {
            await OnBeforeCloseAsync(ct);

            if (_canvasGroup)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_canvasGroup && _useBaseFade)
                await UniTask.WhenAll(PlayCloseAnimationAsync(ct), FadeAsync(_canvasGroup.alpha, 0f, _fadeDuration, ct));
            else
                await PlayCloseAnimationAsync(ct);

            await UnbindAsync(ct);
            await OnAfterCloseAsync(ct);

            Destroy(gameObject);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            await OnBeforeHideAsync(ct);

            if (_canvasGroup)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_canvasGroup && _useBaseFade)
                await UniTask.WhenAll(PlayHideAnimationAsync(ct), FadeAsync(_canvasGroup.alpha, 0f, _fadeDuration, ct));
            else
                await PlayHideAnimationAsync(ct);

            gameObject.SetActive(false);
            await OnAfterHideAsync(ct);
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
            await OnBeforeShowAsync(ct);
            gameObject.SetActive(true);

            if (_canvasGroup)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                if (_useBaseFade) _canvasGroup.alpha = 0f;
            }

            if (_canvasGroup && _useBaseFade)
                await UniTask.WhenAll(PlayShowAnimationAsync(ct), FadeAsync(_canvasGroup.alpha, 1f, _fadeDuration, ct));
            else
                await PlayShowAnimationAsync(ct);

            if (_canvasGroup)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            await OnAfterShowAsync(ct);
        }

        protected virtual UniTask BindAsync(CancellationToken ct) => UniTask.CompletedTask;

        protected virtual UniTask UnbindAsync(CancellationToken ct)
        {
            Disposables.Clear();
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnBeforeBindAsync(CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask OnAfterBindAsync (CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask OnAfterOpenAsync (CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask OnBeforeCloseAsync(CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask OnAfterCloseAsync (CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask OnBeforeHideAsync (CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask OnAfterHideAsync  (CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask OnBeforeShowAsync (CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask OnAfterShowAsync  (CancellationToken ct) => UniTask.CompletedTask;

        protected virtual UniTask PlayOpenAnimationAsync (CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask PlayCloseAnimationAsync(CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask PlayHideAnimationAsync (CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask PlayShowAnimationAsync (CancellationToken ct) => UniTask.CompletedTask;

        private async UniTask FadeAsync(float from, float to, float duration, CancellationToken ct)
        {
            if (!_canvasGroup || duration <= 0f) { if (_canvasGroup) _canvasGroup.alpha = to; return; }
            float t = 0f;
            while (t < duration)
            {
                ct.ThrowIfCancellationRequested();
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            _canvasGroup.alpha = to;
        }

        protected virtual void OnDestroy() => Disposables.Dispose();
    }
}
