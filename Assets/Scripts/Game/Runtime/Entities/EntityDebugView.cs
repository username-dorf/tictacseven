using UnityEditor;
using UnityEngine;

namespace Game.Entities
{
    public class EntityDebugView : MonoBehaviour
    {
        [SerializeField] private bool _drawValue;
        private int _value;
        
        public void SetValue(int value)
        {
            _value = value;
        }
        
        private void OnDrawGizmos()
        {
            if (_value>0 && _drawValue)
            {
                Gizmos.color = Color.yellow;
                Handles.Label(transform.position + Vector3.up * 0.1f, $"[{_value}]");
            }
        }
    }
}