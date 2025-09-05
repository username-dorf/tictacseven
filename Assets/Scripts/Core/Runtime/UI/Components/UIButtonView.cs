using System;
using Core.Audio;
using Core.Audio.Signals;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [Inject] private SignalBus _signalBus;

    public void Initialize(Action callback)
    {
        button.OnClickAsObservable()
            .Subscribe(_ =>
            {
                callback?.Invoke();
                _signalBus.Fire(new SignalSfxPlay(SfxKey.Ui_Click));
            })
            .AddTo(this);
    }
}
