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
        
        public List<(Vector3 start, Vector3 end)> CreateLines(BoxCollider collider, int rows, int columns, float spacing = 0.25f)
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
    }
}
