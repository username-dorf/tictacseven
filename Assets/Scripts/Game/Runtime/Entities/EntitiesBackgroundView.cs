using UnityEngine;

namespace Game.Entities
{
    public class EntitiesBackgroundView : MonoBehaviour
    {
        [field: SerializeField] public BoxCollider Collider { get; private set; }
        [field: SerializeField] public EntitiesBackgroundDebugView DebugView { get; private set; }
    }
}