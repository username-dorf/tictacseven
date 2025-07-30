using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entities;
using UniRx;
using UnityEngine;

namespace Game.Field
{
    public interface IReleasePublisher<out T> where T : IPlaceableModel
    {
        IObservable<T> ReleaseCommand { get; }
    }

    public interface IPlaceableModelValue
    {
        ReadOnlyReactiveProperty<int> Value { get;}
        ReadOnlyReactiveProperty<int> Owner { get; }
    }
    
    public interface IPlaceableModel : IPlaceableModelValue
    {
        public interface ITransform
        {
            ReactiveProperty<Vector3> Position { get; }
            ReadOnlyReactiveProperty<Vector3> InitialPosition { get; }
            
            /// <summary>
            /// Indicates if it can be moved, usually after placement.
            /// </summary>
            ReadOnlyReactiveProperty<bool> IsLocked { get; }
            void SetPosition(Vector3 position);
            void SetLocked(bool locked);
        }
        
        ITransform Transform { get; }
    }
    
    public class FieldViewModel : IDisposable
    {
        private CompositeDisposable _disposables;
        private FieldModel _model;

        public FieldViewModel(FieldModel model, IEnumerable<IReleasePublisher<IPlaceableModel>> placeablePublishers)
        {
            _model = model;
            _disposables = new CompositeDisposable();
            
            foreach (var pub in placeablePublishers)
            {
                pub.ReleaseCommand
                    .Subscribe(OnPlaceAttempt)
                    .AddTo(_disposables);
            }
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void OnPlaceAttempt(IPlaceableModel placeableModel)
        {
            var placed = TryPlaceEntity(placeableModel);
            if(placed)
                CheckWinningCondition(_model);
        }

        private void CheckWinningCondition(FieldModel model)
        {
            var winner = GetWinner(model);
            if(winner != null)
                Debug.Log($"Winner found {winner}");
        }

        private bool TryPlaceEntity(IPlaceableModel placeableModel)
        {
            void ResetPlaceablePosition()
            {
                placeableModel.Transform.SetPosition(placeableModel.Transform.InitialPosition.Value);
            }
            try
            {
                var nearestCoors = FindNearestPlace(_model.Entities, placeableModel);
                var nearestPlace = _model.Entities[nearestCoors];
                if (!CanPlace(nearestPlace, placeableModel))
                {
                    Debug.Log("Cannot place entity here: " + nearestPlace.Value.Value);
                    ResetPlaceablePosition();
                    return false;
                }
                var placePosition = nearestPlace.Transform.Position.Value;
                
                placeableModel.Transform.SetPosition(placePosition);
                placeableModel.Transform.SetLocked(true);
                _model.UpdateEntity(nearestCoors,new EntityModel(placeableModel.Value.Value,placeableModel.Owner.Value, placePosition));
                return true;
            }
            catch (Exception e)
            {
                ResetPlaceablePosition();
                Debug.LogException(e);
                return false;
            }
        }

        private Vector2Int FindNearestPlace(IEnumerable<KeyValuePair<Vector2Int, EntityModel>> places, IPlaceableModel placeableModel)
        {
            var maxDistance = FieldConfig.PLACE_MAGNITUDE;
            var key = new Vector2Int(-1, -1);
            float minDist = float.MaxValue;
            Vector3 origin = placeableModel.Transform.Position.Value;

            foreach (var (coors, model) in places)
            {
                float dist = Vector3.Distance(model.Transform.Position.Value, origin);
                if (dist < minDist)
                {
                    minDist = dist;
                    Debug.Log($"Found closer place at ({coors}) with distance: {minDist} ");
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
            return placeModel.Value.Value < placeableModel.Value.Value;
        }

        private bool IsValidOwner(EntityModel placeModel, IPlaceableModel placeableModel)
        {
            if (placeModel.IsEmptyOwner())
                return true;
            if (placeModel.Owner.Value == placeableModel.Owner.Value)
                return false;
            return true;
        }

        public int? GetWinner(FieldModel model)
        {
            var dict = model.Entities;
            var coords = dict.Select(x=>x.Key).ToArray();
            int rows = coords.Max(c => c.x) + 1;
            int columns = coords.Max(c => c.y) + 1;
            for (int row = 0; row < rows; row++)
            {
                var first = dict[new Vector2Int(row, 0)].Owner.Value;
                if (first <= 0) 
                    continue;

                bool allSame = true;
                for (int col = 1; col < columns; col++)
                {
                    if (dict[new Vector2Int(row, col)].Owner.Value != first)
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
                var first = dict[new Vector2Int(0, col)].Owner.Value;
                if (first <= 0)
                    continue;

                bool allSame = true;
                for (int row = 1; row < rows; row++)
                {
                    if (dict[new Vector2Int(row, col)].Owner.Value != first)
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
                var first = dict[new Vector2Int(0, 0)].Owner.Value;
                if (first > 0)
                {
                    bool allSame = true;
                    for (int i = 1; i < rows; i++)
                    {
                        if (dict[new Vector2Int(i, i)].Owner.Value != first)
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
                var first = dict[new Vector2Int(0, columns - 1)].Owner.Value;
                if (first > 0)
                {
                    bool allSame = true;
                    for (int i = 1; i < rows; i++)
                    {
                        var key = new Vector2Int(i, columns - 1 - i);
                        if (dict[key].Owner.Value != first)
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