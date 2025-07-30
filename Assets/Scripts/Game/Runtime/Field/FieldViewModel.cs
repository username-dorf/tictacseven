using System;
using System.Collections.Generic;
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
            void ResetPlaceablePosition()
            {
                placeableModel.Transform.SetPosition(placeableModel.Transform.InitialPosition.Value);
            }
            try
            {
                ref var nearestPlace = ref FindNearestPlace(_model.Entities, placeableModel);
                if (!CanPlace(nearestPlace, placeableModel))
                {
                    Debug.Log("Cannot place entity here: " + nearestPlace.Value.Value);
                    ResetPlaceablePosition();
                    return;
                }
                var placePosition = nearestPlace.Transform.Position.Value;
                
                placeableModel.Transform.SetPosition(placePosition);
                placeableModel.Transform.SetLocked(true);
                nearestPlace = new EntityModel(placeableModel.Value.Value,placeableModel.Owner.Value, placePosition);
            }
            catch (Exception e)
            {
                ResetPlaceablePosition();
                Debug.LogException(e);
            }
        }

        private ref EntityModel FindNearestPlace(EntityModel[,] places, IPlaceableModel placeableModel)
        {
            var maxDistance = FieldConfig.PLACE_MAGNITUDE;
            int rows = places.GetLength(0);
            int cols = places.GetLength(1);

            int foundI = -1;
            int foundJ = -1;
            float minDist = float.MaxValue;
            Vector3 origin = placeableModel.Transform.Position.Value;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var candidate = places[i, j];
                    float dist = Vector3.Distance(candidate.Transform.Position.Value, origin);
                    Debug.Log($"Checking place {candidate.Transform.Position.Value} at (" + i + ", " + j + ") with distance: " + dist);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        Debug.Log("Found closer place at (" + i + ", " + j + ") with distance: " + minDist);
                        foundI = i;
                        foundJ = j;
                    }
                }
            }

            if (foundI < 0 || minDist > maxDistance)
                throw new InvalidOperationException("No suitable place found for the entity.");

            return ref places[foundI, foundJ];
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

        
    }
}