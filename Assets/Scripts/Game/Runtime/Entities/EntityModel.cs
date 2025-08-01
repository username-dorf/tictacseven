using System;
using Game.Field;
using UniRx;
using UnityEngine;

namespace Game.Entities
{
    
    public struct EntityModel : IPlaceableModel, IDisposable
    {
        public const int EMPTY_OWNER = 0;

        public IPlaceableModel.IData Data { get; }
        public IPlaceableModel.ITransform Transform { get; }
        public IPlaceableModel.IEvents Events { get; }


        public EntityModel(int value,int owner, Vector3 position)
        {
            Transform = new EntityTransformModel(position);
            Data = new EntityDataModel(value, owner);
            Events = new EntityEventsModel();
        }
        public static EntityModel CreateEmpty(Vector3 position)
        {
            return new EntityModel(0,EMPTY_OWNER, position);
        }

        public bool IsEmptyOwner()
        {
            return Data.Owner.Value == EMPTY_OWNER;
        }
        
        public struct EntityDataModel : IPlaceableModel.IData
        {
            public ReadOnlyReactiveProperty<int> Merit { get; }
            public ReadOnlyReactiveProperty<int> Owner { get; }
        
            public EntityDataModel(int value, int owner)
            {
                Merit = new ReadOnlyReactiveProperty<int>(new ReactiveProperty<int>(value));
                Owner = new ReadOnlyReactiveProperty<int>(new ReactiveProperty<int>(owner));
            }

            public void Dispose()
            {
                Merit?.Dispose();
                Owner?.Dispose();
            }
        }
        public class EntityTransformModel : IPlaceableModel.ITransform
        {
            public ReadOnlyReactiveProperty<Vector3> Position { get; private set; }
            public ReadOnlyReactiveProperty<Vector3> InitialPosition { get; private set; }
            public ReadOnlyReactiveProperty<bool> Moveable { get; }
            public ReadOnlyReactiveProperty<bool> IsSelected { get; }
            public ReadOnlyReactiveProperty<bool> IsMoving { get; }

            private ReactiveProperty<Vector3> _position;
            private ReactiveProperty<bool> _isLocked;
            private ReactiveProperty<bool> _isSelected;
            private ReactiveProperty<bool> _isMoving;

            
            public EntityTransformModel(Vector3 position)
            {
                _position = new ReactiveProperty<Vector3>(position);
                InitialPosition = new ReadOnlyReactiveProperty<Vector3>(new ReactiveProperty<Vector3>(position));
                
                _isLocked = new ReactiveProperty<bool>(false);
                _isSelected = new ReactiveProperty<bool>(false);
                _isMoving = new ReactiveProperty<bool>(false);
                
                Position = new ReadOnlyReactiveProperty<Vector3>(_position);
                Moveable = new ReadOnlyReactiveProperty<bool>(_isLocked);
                IsSelected = new ReadOnlyReactiveProperty<bool>(_isSelected);
                IsMoving = new ReadOnlyReactiveProperty<bool>(_isMoving);
            }

            public void SetPosition(Vector3 position)
            {
                _position.Value = position;
            }
              
            public void SetLocked(bool locked)
            {
                _isLocked.Value = locked;
            }

            public void SetSelected(bool selected)
            {
                _isSelected.Value = selected;
            }

            public void SetMoving(bool isMoving)
            {
                _isMoving.Value = isMoving;
            }


            public void Dispose()
            {
                _isLocked?.Dispose();
                _isSelected?.Dispose();
                Position?.Dispose();
                InitialPosition?.Dispose();
                Moveable?.Dispose();
                IsSelected?.Dispose();
            }
        }
        
        public class EntityEventsModel : IPlaceableModel.IEvents
        {
            public ReactiveCommand<IPlaceableModel> ReleaseCommand { get; }
            public ReactiveCommand<IPlaceableModel> ReleaseApprovedCommand { get; }

            public EntityEventsModel()
            {
                ReleaseCommand = new ReactiveCommand<IPlaceableModel>();
                ReleaseApprovedCommand = new ReactiveCommand<IPlaceableModel>();
            }
            public void Dispose()
            {
                ReleaseCommand?.Dispose();
            }
        }

        public void Dispose()
        {
            Data?.Dispose();
            Transform?.Dispose();
        }
    }
}