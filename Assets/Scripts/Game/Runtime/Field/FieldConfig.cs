using Game.Entities;
using UnityEngine;

namespace Game.Field
{
    public class FieldConfig
    {
        public const int FIELD_ROWS = 3;
        public const int FIELD_COLUMNS = 3;

        public const int ENTITIES_COUNT = 7;
        public const float PLACE_MAGNITUDE = 1.5f;

        public static readonly EntityPlacedModel[] PRESPAWN_PRESET_1 = new[]
        {
            new EntityPlacedModel(1, 99, new Vector2Int(0, 0)),
        };
    }
}