using Game.Entities;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Field
{
    public class FieldConfig
    {
        public const int FIELD_ROWS = 3;
        public const int FIELD_COLUMNS = 3;

        public const int ENTITIES_COUNT = 7;
        public const float PLACE_MAGNITUDE = 1.5f;

        [NotNull]
        public static EntityPlacedModel[] CREATE_PRESPAWN_PRESET_1(int owner)
        {
            return new[]
            {
                new EntityPlacedModel(1, owner, new Vector2Int(0, 0)),
            };
        }
    }
}