using UnityEngine;

namespace Game.Field
{
    public class FieldView : MonoBehaviour
    {
        [field: SerializeField] public FieldDebugView DebugView { get; private set; }
        [field: SerializeField] public BoxCollider Collider { get; private set; }
    }
}