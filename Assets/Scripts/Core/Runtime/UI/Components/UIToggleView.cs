using PrimeTween;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public class UIToggleView : MonoBehaviour
    {
        [SerializeField] private Sprite enabledSprite;
        [SerializeField] private Sprite disabledSprite;

        [SerializeField] private Image targetGraphic;
        [SerializeField] private UIButtonView button;
        
        [Header("Bounce")]
        [SerializeField] private RectTransform bounceTarget;
        [SerializeField,Range(0.85f, 0.99f)] private float pressScale = 0.92f;
        [SerializeField,Range(1.00f, 1.15f)] private float overshootScale = 1.06f;
        [SerializeField] private float pressDuration = 0.06f;
        [SerializeField] private float overshootDuration = 0.08f;
        [SerializeField] private float settleDuration = 0.06f;
        
        private Sequence _bounceSeq;


        public void Initialize(UIToggleViewModel viewModel)
        {
            if (bounceTarget == null && targetGraphic != null)
                bounceTarget = targetGraphic.rectTransform;
            
            viewModel.Enabled
                .Subscribe(OnChange)
                .AddTo(this);

            button.Initialize(() =>
            {
                PlayBounce();
                viewModel.InvertValue();
            });
        }

        private void OnChange(bool value)
        {
            var sprite = value ? enabledSprite : disabledSprite;
            targetGraphic.sprite = sprite;
        }
        
        private void PlayBounce()
        {
            if (bounceTarget == null)
                return;

            if (_bounceSeq.isAlive)
                _bounceSeq.Stop();

            _bounceSeq = Sequence.Create()
                .Chain(Tween.Scale(bounceTarget, Vector3.one * pressScale, pressDuration, Ease.OutQuad))
                .Chain(Tween.Scale(bounceTarget, Vector3.one * overshootScale, overshootDuration, Ease.OutQuad))
                .Chain(Tween.Scale(bounceTarget, Vector3.one, settleDuration, Ease.OutQuad));
        }

        private void OnDestroy()
        {
            if (_bounceSeq.isAlive)
                _bounceSeq.Stop();
        }
    }

    public class UIToggleViewModel
    {
        public ReactiveProperty<bool> Enabled { get; }

        public UIToggleViewModel(ReactiveProperty<bool> value)
        {
            Enabled = value;
        }

        public void InvertValue()
        {
            Enabled.Value = !Enabled.Value;
        }
    }
}