using UnityEngine;

namespace Game.Field
{
    public interface IFieldView
    {
        Plane DragPlane { get; }
    }

    public class FieldViewProvider
    {
        public IFieldView View { get; private set; }
        public bool Initialized { get; private set; }
        public void Initialize(IFieldView view)
        {
            if (Initialized)
                return;

            View = view;
            Initialized = true;
        }
    }
    public class FieldView : MonoBehaviour, IFieldView
    {
        [field: SerializeField] public FieldDebugView DebugView { get; private set; }
        [field: SerializeField] public BoxCollider Collider { get; private set; }
        public Plane DragPlane { get; private set; }
        
        private void Awake()
        {
            Vector3 planePoint = Collider != null
                ? Collider.bounds.center + Collider.bounds.extents
                : transform.position;
            DragPlane = new Plane(Vector3.up, planePoint+new Vector3(0,0.5f,0));
        }
    }
}