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
    }
}