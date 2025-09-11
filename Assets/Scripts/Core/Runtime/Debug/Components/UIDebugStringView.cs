using TMPro;
using UniRx;
using UnityEngine;

namespace Core.AppDebug.Components
{
    public class UIDebugStringView : MonoBehaviour
    {
        [SerializeField] private TMP_Text debugText;

        public void Initialize(ReactiveProperty<string> value)
        {
            value.Subscribe(OnValueChanged)
                .AddTo(this);
        }
        private void OnValueChanged(string value)
        {
            debugText.text = value;
        }
    }
}