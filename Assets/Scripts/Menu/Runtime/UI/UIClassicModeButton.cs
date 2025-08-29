using Cysharp.Threading.Tasks;
using Menu.UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class UIClassicModeButton : MonoBehaviour
{
    [SerializeField] private Button classicModeButton;

    public void Initialize(ModeButtonViewModel vm)
    {
        var ct = this.GetCancellationTokenOnDestroy();

        classicModeButton.OnPointerClickAsObservable()
            .Select(_ => vm.ExecuteAsync(ct).ToObservable())
            .Concat()
            .Subscribe(_ => { }, Debug.LogException)
            .AddTo(this);
    }
}