using System;
using Game.Field;
using UniRx;
using UnityEngine;

namespace Game.Entities
{
    
    public struct EntityModel : IPlaceableModel, IDisposable
    {
        public ReadOnlyReactiveProperty<int> Value { get; private set; }
        public IPlaceableModel.ITransform Transform { get; private set; }

        public EntityModel(int value, Vector3 position)
        {
            Value = new ReadOnlyReactiveProperty<int>(new ReactiveProperty<int>(value));
            Transform = new EntityTransformModel(position);
        }
        public static EntityModel CreateEmpty(Vector3 position)
        {
            return new EntityModel(0, position);
        }

        public class EntityTransformModel : IPlaceableModel.ITransform
        {
            public ReactiveProperty<Vector3> Position { get; private set; }
            public ReadOnlyReactiveProperty<Vector3> InitialPosition { get; private set; }
            
            public EntityTransformModel(Vector3 position)
            {
                Position = new ReactiveProperty<Vector3>(position);
                InitialPosition = new ReadOnlyReactiveProperty<Vector3>(new ReactiveProperty<Vector3>(position));
            }

            public void SetPosition(Vector3 position)
            {
                Position.Value = position;
            }
        }

        public void Dispose()
        {
            Value?.Dispose();
        }
    }
}