using System.Collections.Generic;
using UnityEngine;

namespace Game.Field
{
    public class FieldDebugView : MonoBehaviour
    {
        [SerializeField] private bool drawPoints = false;
        [SerializeField] private bool drawLines = false;
        [SerializeField] private bool drawPaths = false;

        private Vector3[,] _debugPoints;
        private List<(Vector3 start, Vector3 end)> _debugLines;
        private List<Vector3>[,] _debugPaths;

        public void SetPoints(Vector3[,] newPoints)
        {
            _debugPoints = newPoints;
        }

        public void SetPaths(List<Vector3>[,] paths)
        {
            _debugPaths = paths;
        }

        public void SetLines(List<(Vector3 start, Vector3 end)> newLines)
        {
            _debugLines = newLines;
        }

#if UNITY_EDITOR

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
                        UnityEditor.Handles.Label(point + Vector3.up * 0.1f, $"[{x},{y}]");
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

            if (_debugPaths != null && drawPaths)
            {
                int rows = _debugPaths.GetLength(0);
                int cols = _debugPaths.GetLength(1);

                for (int x = 0; x < rows; x++)
                {
                    for (int y = 0; y < cols; y++)
                    {
                        var path = _debugPaths[x, y];
                        if (path != null && path.Count > 1)
                        {
                            DrawPath(path, x, y);
                        }
                    }
                }
            }
        }

        private void DrawPath(List<Vector3> path, int row, int col)
        {
            float hue = ((float) (row * _debugPaths.GetLength(1) + col) /
                         (_debugPaths.GetLength(0) * _debugPaths.GetLength(1))) % 1f;
            Gizmos.color = Color.HSVToRGB(hue, 0.8f, 1f);

            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }

            if (path.Count > 2)
            {
                Gizmos.DrawLine(path[path.Count - 1], path[0]);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(path[0], 0.02f);

            if (path.Count > 1)
            {
                Vector3 direction = (path[1] - path[0]).normalized;
                Vector3 arrowPos = path[0] + direction * 0.1f;

                Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * 0.05f;
                Gizmos.color = Color.white;
                Gizmos.DrawLine(arrowPos, arrowPos - direction * 0.05f + perpendicular);
                Gizmos.DrawLine(arrowPos, arrowPos - direction * 0.05f - perpendicular);
            }
        }
#endif
    }
}