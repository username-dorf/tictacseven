using System;
using Game.Field;
using UniRx;
using UnityEngine;

namespace Game.Entities
{
    
    public struct EntityModel : IPlaceableModel, IDisposable
    {
        private const int EMPTY_OWNER = 0;
        public ReadOnlyReactiveProperty<int> Value { get; private set; }
        public ReadOnlyReactiveProperty<int> Owner { get; }
        public IPlaceableModel.ITransform Transform { get; private set; }
        

        public EntityModel(int value,int owner, Vector3 position)
        {
            Value = new ReadOnlyReactiveProperty<int>(new ReactiveProperty<int>(value));
            Transform = new EntityTransformModel(position);
            Owner = new ReadOnlyReactiveProperty<int>(new ReactiveProperty<int>(owner));
            
        }
        public static EntityModel CreateEmpty(Vector3 position)
        {
            return new EntityModel(0,EMPTY_OWNER, position);
        }

        public bool IsEmptyOwner()
        {
            return Owner.Value == EMPTY_OWNER;
        }
      

        public class EntityTransformModel : IPlaceableModel.ITransform
        {
            public ReactiveProperty<Vector3> Position { get; private set; }
            public ReadOnlyReactiveProperty<Vector3> InitialPosition { get; private set; }
            public ReadOnlyReactiveProperty<bool> IsLocked { get; }

            private ReactiveProperty<bool> _isLocked;

            
            public EntityTransformModel(Vector3 position)
            {
                Position = new ReactiveProperty<Vector3>(position);
                InitialPosition = new ReadOnlyReactiveProperty<Vector3>(new ReactiveProperty<Vector3>(position));
                _isLocked = new ReactiveProperty<bool>(false);
                IsLocked = new ReadOnlyReactiveProperty<bool>(_isLocked);
            }

            public void SetPosition(Vector3 position)
            {
                Position.Value = position;
            }
              
            public void SetLocked(bool locked)
            {
                _isLocked.Value = locked;
            }
        }

        public void Dispose()
        {
            Value?.Dispose();
            Owner?.Dispose();
        }
    }
}