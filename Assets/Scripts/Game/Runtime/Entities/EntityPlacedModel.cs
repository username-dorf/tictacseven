using Game.Field;
using UniRx;
using UnityEngine;

namespace Game.Entities
{
    public struct EntityPlacedModel : IPlaceableModelValue
    {
        public ReadOnlyReactiveProperty<Vector2Int> GridPosition { get; }
        public ReadOnlyReactiveProperty<int> Value { get; }
        public ReadOnlyReactiveProperty<int> Owner { get; }

        public EntityPlacedModel(int value, int owner, Vector2Int gridPosition)
        {
            Value = new ReadOnlyReactiveProperty<int>(new ReactiveProperty<int>(value));
            Owner = new ReadOnlyReactiveProperty<int>(new ReactiveProperty<int>(owner));
            GridPosition = new ReadOnlyReactiveProperty<Vector2Int>(new ReactiveProperty<Vector2Int>(gridPosition));
        }
    }
}