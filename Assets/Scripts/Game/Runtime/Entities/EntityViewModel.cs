using System;
using UniRx;

namespace Game.Entities
{
    public class EntityViewModel: IDisposable
    {
        public ReactiveProperty<int> Value { get; private set; }
        
        public EntityViewModel(EntityModel model)
        {
            Value = new ReactiveProperty<int>(model.Value);
        }
        public void Dispose()
        {
            Value?.Dispose();
        }
    }
}