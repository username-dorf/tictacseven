using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonView : MonoBehaviour
{
    [SerializeField] private Button button;

    public void Initialize(Action callback)
    {
        button.OnClickAsObservable()
            .Subscribe(_ => callback?.Invoke())
            .AddTo(this);
    }
}
