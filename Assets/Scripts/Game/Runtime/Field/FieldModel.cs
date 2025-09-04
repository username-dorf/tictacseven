using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Entities;
using Game.User;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Field
{
    public class FieldModel: IDisposable
    {
        public IReadOnlyReactiveDictionary<Vector2Int, EntityModel> Entities => _entities;
        public IObservable<(Vector2Int,EntityModel)> OnEntityChanged => _onEntityChanged;
        
        private ReactiveDictionary<Vector2Int, EntityModel> _entities;
        private ReactiveDictionary<Vector2Int, IPlaceableModel> _placebles = new();
        
        private Subject<(Vector2Int,EntityModel)> _onEntityChanged = new();

        private Vector3[,] _gridCache;

        [Inject]
        public FieldModel(int row, int column)
        {
            var matrix = new Vector3[row, column];
            _gridCache = matrix;
            _entities = CreateEmpty(matrix);
        }
        public FieldModel(Vector3[,] grid)
        {
            _gridCache = grid;
            _entities = CreateEmpty(grid);
        }

        public FieldModel(Vector3[,] grid, EntityPlacedModel[] placedModels)
        {
            _gridCache = grid;
            if (placedModels is null || placedModels.Length == 0)
            {
                _entities = CreateEmpty(grid);
                return;
            }

            _entities = CreateEmpty(grid);
            foreach (var placedModel in placedModels)
            {
                var coors = placedModel.GridPosition.Value;
                var position = _entities[coors].Transform.Position;
                _entities[coors] = new EntityModel(placedModel.Data.Merit.Value, placedModel.Data.Owner.Value,
                    position.Value);
            }
        }

        public float[] BuildState(
            UserEntitiesModel player,
            UserEntitiesModel opponent,
            int unityCurrentPlayer
        )
        {
            int egoOwnerId = (unityCurrentPlayer == 2) ? 2 : 1;

            var boardValues = new float[9];
            var boardOwners = new float[9];

            int k = 0;
            foreach (var kvp in Entities.OrderBy(kv => kv.Key.x).ThenBy(kv => kv.Key.y))
            {
                var cell = kvp.Value;

                int o = cell.Data.Owner.Value;
                int ownerEgo = (o == 0) ? 0 : (o == egoOwnerId ? +1 : -1);

                int vAbs = Math.Abs(cell.Data.Merit.Value);
                boardOwners[k] = ownerEgo;
                boardValues[k] = vAbs * ownerEgo;
                k++;
            }

            var ownRem = new float[7];
            foreach (var e in player.Entities)
            {
                int v = e.Data.Merit.Value;
                if (v >= 1 && v <= 7) ownRem[v - 1] = 1f;
            }

            var oppRem = new float[7];
            foreach (var e in opponent.Entities)
            {
                int v = e.Data.Merit.Value;
                if (v >= 1 && v <= 7) oppRem[v - 1] = 1f;
            }

            var state = new float[32];
            Array.Copy(boardValues, 0, state, 0, 9); // [0..8]
            Array.Copy(boardOwners, 0, state, 9, 9); // [9..17]
            Array.Copy(ownRem, 0, state, 18, 7); // [18..24]
            Array.Copy(oppRem, 0, state, 25, 7); // [25..31]

            return state;
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

        public void UpdateEntity(Vector2Int coors, Vector3 position, IPlaceableModel placeable)
        {
            var model = new EntityModel(placeable.Data.Merit.Value, placeable.Data.Owner.Value, position);
            _entities[coors] = model;
            _onEntityChanged?.OnNext((coors,model));
            TryReplacePlaced(coors, placeable);
        }

        private bool TryReplacePlaced(Vector2Int coors, IPlaceableModel placeable)
        {
            var contains = _placebles.ContainsKey(coors);
            if (contains)
                _placebles[coors].Transform.SetVisible(!contains);
            _placebles[coors] = placeable;
            return contains;
        }

        public void Drop()
        {
            _entities?.Clear();
            _entities = CreateEmpty(_gridCache);
            _placebles.Clear();
        }

        public void Dispose()
        {
            _entities?.Dispose();
            _placebles?.Dispose();
            _onEntityChanged?.Dispose();
        }

        public Dictionary<Vector2Int, EntityModel.EntityDataSnapshot> GetDataSnapshot()
        {
            var dict = new Dictionary<Vector2Int, EntityModel.EntityDataSnapshot>(_entities.Count);

            foreach (var kv in _entities)
            {
                var key = kv.Key;
                var entity = kv.Value;

                if (entity.Data is EntityModel.EntityDataModel data)
                {
                    dict[key] = new EntityModel.EntityDataSnapshot(data);
                }
                else                                                      
                {
                    var merit = entity.Data?.Merit?.Value ?? 0;
                    var owner = entity.Data?.Owner?.Value ?? EntityModel.EMPTY_OWNER;
                    dict[key] = new EntityModel.EntityDataSnapshot(owner,merit);
                }
            }

            return dict;
        }

        public void OverrideValues(ReadOnlyDictionary<Vector2Int, EntityModel.EntityDataModel> input)
        {
            if (input == null) return;

            foreach (var kv in input)
            {
                if (!_entities.TryGetValue(kv.Key, out var cur))
                    continue;

                int newMerit = kv.Value.Merit.Value;
                int newOwner = kv.Value.Owner.Value;

                if (cur.Data.Merit.Value == newMerit && cur.Data.Owner.Value == newOwner)
                    continue;

                var pos = cur.Transform.Position.Value;

                var updated = new EntityModel(newMerit, newOwner, pos);
                _entities[kv.Key] = updated;

                //_onEntityChanged?.OnNext((kv.Key, updated));

                if (_placebles.TryGetValue(kv.Key, out var placed))
                {
                    placed.Transform.SetVisible(false);
                    _placebles.Remove(kv.Key);
                }
            }
        }

        public bool FindDifference(Dictionary<Vector2Int, EntityModel.EntityDataSnapshot> input, out (Vector2Int coors, EntityModel.EntityDataSnapshot snapshot) difference)
        {
            if (input == null)
            {
                difference = default;
                return false;
            }

            foreach (var kv in input)
            {
                if (!_entities.TryGetValue(kv.Key, out var cur))
                    continue;

                var other = kv.Value;
                if (cur.Data.Owner.Value != other.Owner ||
                    cur.Data.Merit.Value != other.Merit)
                {
                    difference = (kv.Key, other);
                    return true;
                }
            }
            difference = default;
            return false;
        }
    }
}