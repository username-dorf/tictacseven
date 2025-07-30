using Game.Entities;
using UnityEngine;

namespace Game.Field
{
    public class FieldModel
    {
        public EntityModel[,] Entities;

        public FieldModel(Vector3[,] grid)
        {
            Entities = new EntityModel[grid.GetLength(0), grid.GetLength(1)];
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int column = 0; column < grid.GetLength(1); column++)
                {
                    Entities[row, column] = EntityModel.CreateEmpty(grid[row, column]);
                }
            }
        }
        
        public void SetValue(int row, int column, EntityModel value)
        {
            Entities[row, column] = value;
        }
    }
}