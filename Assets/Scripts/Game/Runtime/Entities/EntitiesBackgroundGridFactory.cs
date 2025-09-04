using System.Linq;
using Game.Field;
using UnityEngine;

namespace Game.Entities
{
    public class EntitiesBackgroundGridFactory
    {
        private const float BORDER_SPACING = 1f;

        public Vector3[] Create(BoxCollider boxCollider, int amount)
        {
            var transform = boxCollider.transform;

            Vector3 size = Vector3.Scale(boxCollider.size, transform.lossyScale);
            Vector3 center = boxCollider.center;

            float start = -size.z / 2f + BORDER_SPACING;
            float end = size.z / 2f - BORDER_SPACING;

            float totalLength = end - start;
            float step = totalLength / amount;

            var result = new Vector3[amount];

            for (int i = 0; i < amount; i++)
            {
                float localZ = end - step * (i + 0.5f);
                Vector3 localPoint = new Vector3(0f, size.y / 2f, localZ) + center;

                result[i] = transform.TransformPoint(localPoint);
            }

            return result;
        }
        private static readonly Vector2Int[] PATTERN_POINTS=
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2),
            new Vector2Int(0, 3),
            new Vector2Int(1, 3),
            new Vector2Int(2, 3),
            new Vector2Int(3, 3),
        };
        private static readonly Vector2Int[] PATTERN_POINTS_INVERTED=
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(3, 0),
            new Vector2Int(3, 1),
            new Vector2Int(3, 2),
            new Vector2Int(3, 3),
        };

        public Vector3[] CreateOnRect(BoxCollider boxCollider, bool inverted=false)
        {
            var pattern = inverted ? PATTERN_POINTS_INVERTED : PATTERN_POINTS;
            return CreateOnRect(boxCollider, 4, 4, pattern);
        }
        private Vector3[] CreateOnRect(BoxCollider boxCollider, int rows, int columns, Vector2Int[] pattern)
        {
            var rectGridFactory = new FieldGridFactory();
            var points = rectGridFactory.Create(boxCollider, rows, columns,1f);
            var selectedPoints = pattern
                .Select(p => new Vector2Int(p.x % columns, p.y % rows))
                .Select(p => points[p.y, p.x])
                .ToArray();
            return selectedPoints;
        }
    }
}