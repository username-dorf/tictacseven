using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public abstract class UISelectorView : MonoBehaviour
    {
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        
        public virtual void Initialize(Action onNext, Action onPrevious)
        {
            previousButton.OnClickAsObservable()
                .Subscribe(_ => onPrevious?.Invoke())
                .AddTo(this);
            
            nextButton.OnClickAsObservable()
                .Subscribe(_ => onNext?.Invoke())
                .AddTo(this);
        }
    }
}