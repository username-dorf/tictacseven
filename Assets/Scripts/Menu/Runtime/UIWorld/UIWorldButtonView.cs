using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Menu.UI;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using static Core.Common.Buttons;

namespace Menu.UIWorld
{
    public class UIWorldButtonView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Press params")] [SerializeField]
        float pressDuration = 0.18f;

        [SerializeField] float pressAmount = 0.22f;
        [SerializeField] float bounceBelow = 0.12f;
        [SerializeField] float xzWiden = 0.08f;

        [Header("Release params")] [SerializeField]
        float releaseDuration = 0.22f;

        [SerializeField] float reboundUp = 0.14f;
        [SerializeField] float xzRelax = 0.06f;

        [Header("Behavior")] 
        [SerializeField] float minPressVisualTime = 0.06f;
        [SerializeField] Vector3 anchor01 = new Vector3(0.5f, 0f, 0.5f);
        
        [Header("Locking")]
        [SerializeField] float afterExecuteCooldown = 0f;

        private bool _isPointerDown;
        private bool _isBusy;
        private float _downTime;
        private CancellationToken _onDestroyToken;
        private ModeButtonViewModel _viewModel;

        private Vector3 _baseScale;
        private Vector3 _basePos;
        private Vector3 _anchorLocal;

        private Sequence _activeSequence;

        public void Initialize(ModeButtonViewModel vm)
        {
            _viewModel = vm;
            _onDestroyToken = this.GetCancellationTokenOnDestroy();

            _baseScale = transform.localScale;
            _basePos = transform.localPosition;
            var b = GetLocalBounds(transform);
            _anchorLocal = AnchorLocalFrom01(b, anchor01);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isBusy) 
                return; 
            
            _isBusy = true;
            _isPointerDown = true;
            _downTime = Time.unscaledTime;

            _activeSequence.Stop();
            _activeSequence = transform.PressBounceFromBase(
                _baseScale, _basePos, _anchorLocal,
                pressDuration, pressAmount, bounceBelow, xzWiden
            );
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPointerDown) return;
            _isPointerDown = false;

            float elapsed = Time.unscaledTime - _downTime;
            _activeSequence.Stop();

            if (elapsed < minPressVisualTime)
            {
                transform.QuickFinishToPressedFromBase(
                    _baseScale, _basePos, _anchorLocal,
                    finishTime: Mathf.Max(0.02f, minPressVisualTime - elapsed),
                    pressAmount: pressAmount, xzWiden: xzWiden
                ).OnComplete(DoRelease);
            }
            else
            {
                DoRelease();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isPointerDown)
                OnPointerUp(eventData);
        }

        private async UniTaskVoid RunActionAndUnlock()
        {
            try
            {
                await _viewModel.ExecuteAsync(_onDestroyToken);
                if (afterExecuteCooldown > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(afterExecuteCooldown), cancellationToken: _onDestroyToken);
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
                _baseScale, _basePos, _anchorLocal,
                releaseDuration, reboundUp, xzRelax
            );
            _activeSequence.OnComplete(() =>
            {
                transform.ReleaseBounceToBase(
                    _baseScale, _basePos, _anchorLocal,
                    0f, 0f, 0f // noop
                );
                RunActionAndUnlock().Forget();
            });
        }
        private void OnDisable()
        {
            _isPointerDown = false;
            _isBusy = false;           
            _activeSequence.Stop();
        }

        private void OnDestroy()
        {
            _activeSequence.Stop();
        }
    }
}