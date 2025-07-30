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

        public FieldModel(Vector3[,] grid, IPlaceableModelValue[,] preset)
        {
            if(grid.GetLength(0) != preset.GetLength(0) || grid.GetLength(1) != preset.GetLength(1))
            {
                throw new System.ArgumentException("Grid and preset dimensions do not match.");
            }
            Entities = new EntityModel[grid.GetLength(0), grid.GetLength(1)];
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int column = 0; column < grid.GetLength(1); column++)
                {
                    var position = grid[row, column];
                    var presetModel = preset[row, column];
                    Entities[row, column] = new EntityModel(presetModel.Value.Value, presetModel.Owner.Value, position);
                }
            }
        }
        
        public void SetValue(int row, int column, EntityModel value)
        {
            Entities[row, column] = value;
        }
    }
}