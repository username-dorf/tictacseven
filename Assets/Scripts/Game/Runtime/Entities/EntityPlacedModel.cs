using Game.Field;
using UniRx;
using UnityEngine;

namespace Game.Entities
{
    public struct EntityPlacedModel
    {
        public ReadOnlyReactiveProperty<Vector2Int> GridPosition { get; }
        public EntityModel.EntityDataModel Data { get; }

        public EntityPlacedModel(int value, int owner, Vector2Int gridPosition)
        {
            Data = new EntityModel.EntityDataModel(value, owner);
            GridPosition = new ReadOnlyReactiveProperty<Vector2Int>(new ReactiveProperty<Vector2Int>(gridPosition));
        }
    }
}