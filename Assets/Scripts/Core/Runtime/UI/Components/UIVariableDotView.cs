using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    [Serializable]
    public class UIVariableDotViewGroup
    {
        [field: SerializeField] public List<UIVariableDotView> Dots { get; private set; }
        private int _currentIndex = 0;

        public async UniTask WarmUp(bool[] results,CancellationToken ct)
        {
            var states = new UIVariableDotView.State[3];
            for (var i = 0; i < results.Length; i++)
            {
                states[i] = results[i]? UIVariableDotView.State.Valid : UIVariableDotView.State.Invalid;
            }
            _currentIndex = results.Length;
            var tasks = new List<UniTask>();
            for (int i = 0; i < states.Length; i++)
            {
                tasks.Add(Dots[i].SetStateAsync(states[i],ct));
            }
            await UniTask.WhenAll(tasks);
        }

        public async UniTask SetLastStateAsync(bool result, CancellationToken ct, bool animate=true)
        {
            var state = result ? UIVariableDotView.State.Valid : UIVariableDotView.State.Invalid;
            if(_currentIndex >= Dots.Count)
                throw new ArgumentOutOfRangeException($"Current index {_currentIndex} exceeds Dots count {Dots.Count}");
            await Dots[_currentIndex].SetStateAsync(state, ct, animate);
            _currentIndex++;
        }

        public void Clear()
        {
            _currentIndex = 0;
        }
        
        public void OnDisable()
        {
            foreach (var dot in Dots)
                if (dot)
                    Tween.StopAll(dot.transform as RectTransform);
            Clear();
        }
    }
    public class UIVariableDotView : MonoBehaviour
    {
        public enum State
        {
            Default,
            Valid,
            Invalid,
        }

        [Header("Sprite Settings")] [SerializeField]
        private Sprite defaultSprite;

        [SerializeField] private Sprite validSprite;
        [SerializeField] private Sprite invalidSprite;
        [Space] [SerializeField] private Image targetGraphic;

        public async UniTask SetStateAsync(State state, CancellationToken ct = default, bool animate = false)
        {
            if (targetGraphic == null) 
                return;

            switch (state)
            {
                case State.Default:
                    targetGraphic.sprite = defaultSprite;
                    break;
                case State.Valid:
                    targetGraphic.sprite = validSprite;
                    break;
                case State.Invalid:
                    targetGraphic.sprite = invalidSprite;
                    break;
            }
            if(animate)
                await PlayAnimation(targetGraphic.rectTransform,ct);
        }

        private async UniTask PlayAnimation(RectTransform rt, CancellationToken ct, float duration = 0.20f, float start = 0.90f, float overshoot = 1.06f)
        {
            Tween.StopAll(rt);
            rt.localScale = Vector3.one * start;
            float up = duration * 0.55f, down = duration - up;
            var sequence = new Sequence();
            try
            {
                sequence = Sequence.Create()
                    .Chain(Tween.Scale(rt, Vector3.one * overshoot, up, Ease.OutCubic))
                    .Chain(Tween.Scale(rt, Vector3.one, down, Ease.OutBack));
                await sequence.ToUniTask(cancellationToken:ct);
            }
            catch (OperationCanceledException e)
            {
                if(sequence.isAlive)
                    sequence.Stop();
            }
        }

        [Button]
        private void _SetValid()
        {
            SetSprite(validSprite);
        }

        [Button]
        private void _SetInvalid()
        {
            SetSprite(invalidSprite);
        }

        [Button]
        private void _SetDefault()
        {
            SetSprite(defaultSprite);
        }

        private void SetSprite(Sprite sprite)
        {
            targetGraphic.sprite = sprite;
        }
    }
}