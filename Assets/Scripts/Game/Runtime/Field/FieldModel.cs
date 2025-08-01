using Game.Entities;
using UniRx;
using UnityEngine;

namespace Game.Field
{
    public class FieldModel
    {
        public IReadOnlyReactiveDictionary<Vector2Int, EntityModel> Entities => _entities;
        private ReactiveDictionary<Vector2Int, EntityModel> _entities;

        public FieldModel(Vector3[,] grid)
        {
            _entities = CreateEmpty(grid);
        }

        public FieldModel(Vector3[,] grid, EntityPlacedModel[] placedModels)
        {
            if(placedModels is null || placedModels.Length==0)
            {
                _entities = CreateEmpty(grid);
                return;
            }
            
            _entities = CreateEmpty(grid);
            foreach (var placedModel in placedModels)
            {
                var coors = placedModel.GridPosition.Value;
                var position = _entities[coors].Transform.Position;
                _entities[coors] = new EntityModel(placedModel.Data.Merit.Value, placedModel.Data.Owner.Value, position.Value);
            }
        }

        private ReactiveDictionary<Vector2Int, EntityModel> CreateEmpty(Vector3[,] grid)
        {
            var result = new ReactiveDictionary<Vector2Int, EntityModel>();
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int column = 0; column < grid.GetLength(1); column++)
                {
                    result.Add(new Vector2Int(row, column), EntityModel.CreateEmpty(grid[row, column]));
                }
            }

            return result;
        }

        public void UpdateEntity(Vector2Int position, EntityModel model)
        {
            _entities[position] = model;
        }

        
    }
}