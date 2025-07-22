using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Field
{
    public class FieldDebugView : MonoBehaviour
    {
        [SerializeField] private bool drawPoints = false;
        [SerializeField] private bool drawLines = false;
        
        private Vector3[,] _debugPoints;
        private List<(Vector3 start, Vector3 end)> _debugLines;


        public void SetPoints(Vector3[,] newPoints)
        {
            _debugPoints = newPoints;
        }
        
        public void SetLines(List<(Vector3 start, Vector3 end)> newLines)
        {
            _debugLines = newLines;
        }

        private void OnDrawGizmos()
        {
            if (_debugPoints != null && drawPoints)
            {
                Gizmos.color = Color.green;

                int rows = _debugPoints.GetLength(0);
                int cols = _debugPoints.GetLength(1);

                for (int x = 0; x < rows; x++)
                {
                    for (int y = 0; y < cols; y++)
                    {
                        var point = _debugPoints[x, y];
                        Gizmos.DrawSphere(point, 0.1f);
#if UNITY_EDITOR
                        Handles.Label(point + Vector3.up * 0.1f, $"[{x},{y}]");
#endif
                    }
                }
            }

            if (_debugLines != null && drawLines)
            {
                Gizmos.color = Color.yellow;

                foreach (var (start, end) in _debugLines)
                {
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}