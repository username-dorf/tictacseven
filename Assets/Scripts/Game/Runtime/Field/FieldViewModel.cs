using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entities;
using Game.User;
using UniRx;
using UnityEngine;

namespace Game.Field
{
    public class FieldViewModel : IDisposable
    {
        private CompositeDisposable _disposables;
        private FieldModel _model;

        public FieldViewModel(FieldModel model, params UserEntitiesModel[] userEntitiesModels)
        {
            _model = model;
            _disposables = new CompositeDisposable();
            
            foreach (var entitiesModel in userEntitiesModels)
            {
                foreach (var pub in entitiesModel.Entities)
                {
                    pub.Events.ReleaseCommand
                        .Subscribe(OnPlaceAttempt)
                        .AddTo(_disposables);
                }
            }
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void OnPlaceAttempt(IPlaceableModel placeableModel)
        {
            var placed = TryPlaceEntity(placeableModel);
        }

        private bool TryPlaceEntity(IPlaceableModel placeableModel)
        {
            void ResetPlaceablePosition()
            {
                placeableModel.Transform.SetPosition(placeableModel.Transform.InitialPosition);
                Debug.Log($"{placeableModel.Data.Merit.Value} by {placeableModel.Data.Owner.Value} " +
                          $"Position {placeableModel.Transform.Position.Value} reset to initial {placeableModel.Transform.InitialPosition}");
            }
            try
            {
                var nearestCoors = FindNearestPlace(_model.Entities, placeableModel);
                var nearestPlace = _model.Entities[nearestCoors];
                if (!CanPlace(nearestPlace, placeableModel))
                {
                    Debug.Log("Cannot place entity here: " + nearestPlace.Data.Merit.Value);
                    ResetPlaceablePosition();
                    return false;
                }
                var placePosition = nearestPlace.Transform.Position.Value;
                
                _model.UpdateEntity(nearestCoors,placePosition, placeableModel);
                placeableModel.Transform.SetPosition(placePosition);
                placeableModel.Transform.SetMoveable(false);
                placeableModel.Events.ReleaseApprovedCommand.Execute(placeableModel);
                return true;
            }
            catch (Exception e)
            {
                ResetPlaceablePosition();
                return false;
            }
        }

        private Vector2Int FindNearestPlace(IEnumerable<KeyValuePair<Vector2Int, EntityModel>> places, IPlaceableModel placeableModel)
        {
            var maxDistance = FieldConfig.PLACE_MAGNITUDE;
            var key = new Vector2Int(-1, -1);
            float minDist = float.MaxValue;
            var originPosition = placeableModel.Transform.Position.Value;
            Vector3 origin = new Vector3(originPosition.x,0,originPosition.z);

            foreach (var (coors, model) in places)
            {
                var targetPosition = model.Transform.Position.Value;
                var target = new Vector3(targetPosition.x, 0, targetPosition.z);
                float dist = Vector3.Distance(target, origin);
                if (dist < minDist)
                {
                    minDist = dist;
                    key = coors;
                }
            }
            if (key.x < 0 || minDist > maxDistance)
                throw new InvalidOperationException("No suitable place found for the entity.");

            return key;
        }

        private bool CanPlace(EntityModel placeModel, IPlaceableModel placeableModel)
        {
            if (!IsValidOwner(placeModel, placeableModel))
                return false;
            return placeModel.Data.Merit.Value < placeableModel.Data.Merit.Value;
        }

        private bool IsValidOwner(EntityModel placeModel, IPlaceableModel placeableModel)
        {
            if (placeModel.IsEmptyOwner())
                return true;
            if (placeModel.Data.Owner.Value == placeableModel.Data.Owner.Value)
                return false;
            return true;
        }
        
    }

    public static class FieldModelExtensions
    {
        public static bool IsDraw(this FieldModel model, UserEntitiesModel hand)
        {
            var dict = model.Entities;

            if (hand?.Entities == null || hand.Entities.Count == 0)
                return true;

            foreach (var cell in dict)
            {
                if (cell.Value.Data.Owner.Value <= 0)
                    return false;
            }

            int maxMyMerit = int.MinValue;
            foreach (var piece in hand.Entities)
            {
                int m = piece.Data.Merit.Value;
                if (m > maxMyMerit) maxMyMerit = m;
            }

            int minEnemyTopMerit = int.MaxValue;
            foreach (var cell in dict)
            {
                int owner = cell.Value.Data.Owner.Value;
                if (owner > 0 && owner != hand.Owner)
                {
                    int enemyMerit = cell.Value.Data.Merit.Value;
                    if (enemyMerit < minEnemyTopMerit) minEnemyTopMerit = enemyMerit;
                }
            }

            if (minEnemyTopMerit == int.MaxValue)
                return true;

            return !(maxMyMerit > minEnemyTopMerit);
        }
    
        public static int? GetWinner(this FieldModel model)
        {
            var dict = model.Entities;
            var coords = dict.Select(x=>x.Key).ToArray();
            int rows = coords.Max(c => c.x) + 1;
            int columns = coords.Max(c => c.y) + 1;
            for (int row = 0; row < rows; row++)
            {
                var first = dict[new Vector2Int(row, 0)].Data.Owner.Value;
                if (first <= 0) 
                    continue;

                bool allSame = true;
                for (int col = 1; col < columns; col++)
                {
                    if (dict[new Vector2Int(row, col)].Data.Owner.Value != first)
                    {
                        allSame = false;
                        break;
                    }
                }

                if (allSame)
                    return first;
            }

            for (int col = 0; col < columns; col++)
            {
                var first = dict[new Vector2Int(0, col)].Data.Owner.Value;
                if (first <= 0)
                    continue;

                bool allSame = true;
                for (int row = 1; row < rows; row++)
                {
                    if (dict[new Vector2Int(row, col)].Data.Owner.Value != first)
                    {
                        allSame = false;
                        break;
                    }
                }

                if (allSame)
                    return first;
            }

            // if the field is not square, we cannot check diagonals
            if (rows != columns)
                return null;

            //  main diagonal (0,0) → (_rows‑1,_cols‑1)
            {
                var first = dict[new Vector2Int(0, 0)].Data.Owner.Value;
                if (first > 0)
                {
                    bool allSame = true;
                    for (int i = 1; i < rows; i++)
                    {
                        if (dict[new Vector2Int(i, i)].Data.Owner.Value != first)
                        {
                            allSame = false;
                            break;
                        }
                    }

                    if (allSame) return first;
                }
            }

            // additional diagonal (_rows‑1,0) → (0,_cols‑1)
            {
                var first = dict[new Vector2Int(0, columns - 1)].Data.Owner.Value;
                if (first > 0)
                {
                    bool allSame = true;
                    for (int i = 1; i < rows; i++)
                    {
                        var key = new Vector2Int(i, columns - 1 - i);
                        if (dict[key].Data.Owner.Value != first)
                        {
                            allSame = false;
                            break;
                        }
                    }

                    if (allSame) return first;
                }
            }
            return null;
        }
    }
}