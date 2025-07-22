using UniRx;
using UnityEngine;

namespace Game.Entities
{
    public class EntityView: MonoBehaviour
    {
        [field: SerializeField] public EntityDebugView DebugView { get; private set; }
        
        public void Initialize(EntityViewModel viewModel)
        {
            viewModel.Value
                .Subscribe(OnValueChanged)
                .AddTo(this);
        }
        
        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }
        
        private void OnValueChanged(int value)
        {
            DebugView.SetValue(value);
        }
    }
}