using System;
using System.Threading;
using Core.Common;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using PrimeTween;
using UnityEngine;

namespace Core.UI.Components
{
    public abstract class UIWorldBounceable: MonoBehaviour
    {
        private const string PRESS_PARAMS_FOLDOUT = "Press params";
        private const string RELEASE_PARAMS_FOLDOUT = "Release params";
        private const string BEHAVIOUR_FOLDOUT = "Behaviour";
        private const string LOCKING_FOLDOUT = "Locking";
        
        [Foldout(PRESS_PARAMS_FOLDOUT), SerializeField] private float pressDuration = 0.18f;

        [Foldout(PRESS_PARAMS_FOLDOUT), SerializeField] private float pressAmount = 0.22f;
        [Foldout(PRESS_PARAMS_FOLDOUT), SerializeField] private float bounceBelow = 0.12f;
        [Foldout(PRESS_PARAMS_FOLDOUT), SerializeField] private float xzWiden = 0.08f;

        [Foldout(RELEASE_PARAMS_FOLDOUT), SerializeField] private float releaseDuration = 0.22f;

        [Foldout(RELEASE_PARAMS_FOLDOUT), SerializeField] private float reboundUp = 0.14f;
        [Foldout(RELEASE_PARAMS_FOLDOUT), SerializeField] private float xzRelax = 0.06f;

        [Foldout(BEHAVIOUR_FOLDOUT), SerializeField] private float minPressVisualTime = 0.06f;
        [Foldout(BEHAVIOUR_FOLDOUT), SerializeField] Vector3 anchor01 = new Vector3(0.5f, 0f, 0.5f);
        
        [Foldout(LOCKING_FOLDOUT), SerializeField] private float afterExecuteCooldown = 0f;

        private bool _isPointerDown;
        private bool _isBusy;
        private float _downTime;

        protected Vector3 BaseScale;
        protected Vector3 BasePos;
        protected Vector3 AnchorLocal;

        private Sequence _activeSequence;
        protected CancellationToken OnDestroyToken;
        
        protected abstract UniTask GetOnReleaseAction(CancellationToken token); 

        public void Initialize()
        {
            OnDestroyToken = this.GetCancellationTokenOnDestroy();

            BaseScale = transform.localScale;
            BasePos = transform.localPosition;
            var meshBounds = transform.GetLocalBounds();
            AnchorLocal = meshBounds.AnchorLocalFrom01(anchor01);
        }
        
        public void ExecutePress()
        {
            if (_isBusy) 
                return; 
            
            _isBusy = true;
            _isPointerDown = true;
            _downTime = Time.unscaledTime;

            _activeSequence.Stop();
            _activeSequence = transform.PressBounceFromBase(
                BaseScale, BasePos, AnchorLocal,
                pressDuration, pressAmount, bounceBelow, xzWiden
            );
        }
        
        public void ExecuteRelease()
        {
            if (!_isPointerDown) return;
            _isPointerDown = false;

            float elapsed = Time.unscaledTime - _downTime;
            _activeSequence.Stop();

            if (elapsed < minPressVisualTime)
            {
                transform.QuickFinishToPressedFromBase(
                    BaseScale, BasePos, AnchorLocal,
                    finishTime: Mathf.Max(0.02f, minPressVisualTime - elapsed),
                    pressAmount: pressAmount, xzWiden: xzWiden
                ).OnComplete(DoRelease);
            }
            else
            {
                DoRelease();
            }
        }

        public void ExecuteExit()
        {
            if (_isPointerDown)
                ExecuteRelease();
        }

        protected async UniTaskVoid RunActionAndUnlock(Func<CancellationToken,UniTask> action)
        {
            try
            {
                await action(OnDestroyToken);
                if (afterExecuteCooldown > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(afterExecuteCooldown), cancellationToken: OnDestroyToken);
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
                _isBusy = false;        
            }
        }
        private void DoRelease()
        {
            _activeSequence = transform.ReleaseBounceToBase(
                BaseScale, BasePos, AnchorLocal,
                releaseDuration, reboundUp, xzRelax
            );
            _activeSequence.OnComplete(() =>
            {
                transform.ReleaseBounceToBase(
                    BaseScale, BasePos, AnchorLocal,
                    0f, 0f, 0f // noop
                );
                RunActionAndUnlock(GetOnReleaseAction)
                    .Forget();
            });
        }
        protected Vector3 GetPressScale()
        {
            float yPressed = BaseScale.y * (1f - pressAmount);
            float xzPressed = BaseScale.x * (1f + xzWiden);
            return new Vector3(xzPressed, yPressed, xzPressed);
        }
        protected virtual void OnDisable()
        {
            _isPointerDown = false;
            _isBusy = false;           
            _activeSequence.Stop();
        }

        protected virtual void OnDestroy()
        {
            _activeSequence.Stop();
        }
    }
}