using UnityEngine;

namespace Game.Entities
{
    public class EntitiesBackgroundDebugView : MonoBehaviour
    {
        [SerializeField] private bool drawPoints;
        private Vector3[] _debugPoints;

        public void SetPoints(Vector3[] newPoints)
        {
            _debugPoints = newPoints;
        }

        private void OnDrawGizmos()
        {
            if (_debugPoints != null && drawPoints)
            {
                Gizmos.color = Color.green;
                foreach (var debugPoint in _debugPoints)
                {
                    Gizmos.DrawSphere(debugPoint, 0.1f);
                }
            }
        }
    }
}