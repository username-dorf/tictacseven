using System.Collections.Generic;
using UnityEngine;

namespace Game.Field
{
    public class FieldGridFactory
    {
        public Vector3[,] Create(BoxCollider collider, int row, int column, float spacing = 0.25f)
        {
            var bounds = collider.bounds;

            var y = bounds.max.y;

            var startX = bounds.min.x + spacing;
            var endX = bounds.max.x - spacing;
            var startZ = bounds.min.z + spacing;
            var endZ = bounds.max.z - spacing;

            var totalWidth = endX - startX;
            var totalDepth = endZ - startZ;

            var cellWidth = totalWidth / column;
            var cellDepth = totalDepth / row;

            var matrix = new Vector3[row, column];

            for (int z = 0; z < row; z++)
            {
                for (int x = 0; x < column; x++)
                {
                    var posX = startX + cellWidth * (x + 0.5f);
                    var posZ = endZ - cellDepth * (z + 0.5f);
                    matrix[z, x] = new Vector3(posX, y, posZ);
                }
            }

            return matrix;
        }

        public Vector3[,] CreateEmpty(int row, int column)
        {
            var matrix = new Vector3[row, column];

            for (int z = 0; z < row; z++)
            {
                for (int x = 0; x < column; x++)
                {
                    matrix[z, x] = Vector3.zero;
                }
            }

            return matrix;
        }

        public List<(Vector3 start, Vector3 end)> CreateLines(BoxCollider collider, int rows, int columns,
            float spacing = 0.25f)
        {
            var bounds = collider.bounds;
            var y = bounds.max.y;

            var startX = bounds.min.x + spacing;
            var endX = bounds.max.x - spacing;
            var startZ = bounds.min.z + spacing;
            var endZ = bounds.max.z - spacing;

            var cellWidth = (endX - startX) / columns;
            var cellDepth = (endZ - startZ) / rows;

            var lines = new List<(Vector3, Vector3)>();

            for (int row = 0; row <= rows; row++)
            {
                var z = startZ + row * cellDepth;
                var start = new Vector3(startX, y, z);
                var end = new Vector3(endX, y, z);
                lines.Add((start, end));
            }

            for (int col = 0; col <= columns; col++)
            {
                var x = startX + col * cellWidth;
                var start = new Vector3(x, y, startZ);
                var end = new Vector3(x, y, endZ);
                lines.Add((start, end));
            }

            return lines;
        }

        public List<Vector3>[,] CreateCellPaths(BoxCollider collider, int row, int column, float cornerRadius = 0.5f,
            float spacing = 0.1f, int pathResolution = 8)
        {
            var bounds = collider.bounds;
            var y = bounds.max.y;

            var startX = bounds.min.x + spacing;
            var endX = bounds.max.x - spacing;
            var startZ = bounds.min.z + spacing;
            var endZ = bounds.max.z - spacing;

            var totalWidth = endX - startX;
            var totalDepth = endZ - startZ;

            var cellWidth = totalWidth / column;
            var cellDepth = totalDepth / row;

            var pathMatrix = new List<Vector3>[row, column];

            for (int z = 0; z < row; z++)
            {
                for (int x = 0; x < column; x++)
                {
                    var centerX = startX + cellWidth * (x + 0.5f);
                    var centerZ = endZ - cellDepth * (z + 0.5f);

                    pathMatrix[z, x] = CreateRoundedRectanglePath(
                        centerX, centerZ, y,
                        cellWidth * 0.8f, cellDepth * 0.8f,
                        cornerRadius, pathResolution
                    );
                }
            }

            return pathMatrix;
        }

        private List<Vector3> CreateRoundedRectanglePath(float centerX, float centerZ, float y,
            float width, float depth, float cornerRadius, int pathResolution)
        {
            var path = new List<Vector3>();
            var maxRadius = Mathf.Min(width, depth) * 0.5f;
            cornerRadius = Mathf.Min(cornerRadius, maxRadius);

            var halfWidth = width * 0.5f;
            var halfDepth = depth * 0.5f;

            var corners = new Vector2[]
            {
                new Vector2(halfWidth - cornerRadius, halfDepth - cornerRadius),
                new Vector2(-halfWidth + cornerRadius, halfDepth - cornerRadius),
                new Vector2(-halfWidth + cornerRadius, -halfDepth + cornerRadius),
                new Vector2(halfWidth - cornerRadius, -halfDepth + cornerRadius)
            };

            var arcStartAngles = new float[] {0f, 90f, 180f, 270f};

            for (int i = 0; i < 4; i++)
            {
                var corner = corners[i];
                var startAngle = arcStartAngles[i] * Mathf.Deg2Rad;

                for (int j = 0; j <= pathResolution; j++)
                {
                    var angle = startAngle + (j * 90f * Mathf.Deg2Rad / pathResolution);
                    var arcX = corner.x + cornerRadius * Mathf.Cos(angle);
                    var arcZ = corner.y + cornerRadius * Mathf.Sin(angle);

                    path.Add(new Vector3(centerX + arcX, y, centerZ + arcZ));
                }
            }

            return path;
        }
    }
}