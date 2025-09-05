using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public abstract class UISelectorView : MonoBehaviour
    {
        [SerializeField] private UIButtonView previousButton;
        [SerializeField] private UIButtonView nextButton;
        
        public virtual void Initialize(Action onNext, Action onPrevious)
        {
            previousButton.Initialize(onPrevious);
            nextButton.Initialize(onNext);
        }
    }
}