using System;
using Game.Field;
using UniRx;
using UnityEngine;

namespace Game.Entities
{
    
    public class EntityModel : IPlaceableModel, IDisposable
    {
        public const int EMPTY_OWNER = 0;

        public IPlaceableModel.IData Data { get; }
        public IPlaceableModel.ITransform Transform { get; }
        public IPlaceableModel.IEvents Events { get; }
        
        public EntityModel()
        {
            
        }
        public EntityModel(int value,int owner, Vector3 position)
        {
            Transform = new EntityTransformModel(position, Vector3.one*0.8f);
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
        
        public class EntityDataModel : IPlaceableModel.IData
        {
            private readonly ReactiveProperty<int> _merit;
            private readonly ReactiveProperty<int> _owner;

            public IReadOnlyReactiveProperty<int> Merit => _merit;
            public IReadOnlyReactiveProperty<int> Owner => _owner;
        
            public EntityDataModel(int value, int owner)
            {
                _merit = new ReactiveProperty<int>(value);
                _owner = new ReactiveProperty<int>(owner);
            }

            public void Dispose()
            {
                _merit?.Dispose();
                _owner?.Dispose();
            }
        }

        public struct EntityDataSnapshot
        {
            public int Owner;
            public int Merit;
            public EntityDataSnapshot(EntityDataModel model)
            {
                Owner = model.Owner.Value;
                Merit = model.Merit.Value;
            }

            public EntityDataSnapshot(int owner, int merit)
            {
                Owner = owner;
                Merit = merit;
            }
        }
        
        public class EntityTransformModel : IPlaceableModel.ITransform
        {
            public ReadOnlyReactiveProperty<bool> Visible { get; }
            public IReadOnlyReactiveProperty<Vector3> Position => _position;
            public IReadOnlyReactiveProperty<Vector3> Scale => _scale;
            public Vector3 InitialPosition { get; }
            public Vector3 InitialScale { get; }
            public ReadOnlyReactiveProperty<bool> IsMoveable { get; }
            public ReadOnlyReactiveProperty<bool> IsSelected { get; }
            public ReadOnlyReactiveProperty<bool> IsMoving { get; }
            public ReadOnlyReactiveProperty<bool> Interactable { get; }


            private ReactiveProperty<Vector3> _position;
            private ReactiveProperty<Vector3> _scale;
            private ReactiveProperty<bool> _isMoveable;
            private ReactiveProperty<bool> _isSelected;
            private ReactiveProperty<bool> _interactable;
            private ReactiveProperty<bool> _isMoving;
            private ReactiveProperty<bool> _isVisible;

            
            public EntityTransformModel(Vector3 position, Vector3 scale)
            {
                _position = new ReactiveProperty<Vector3>(position);
                _scale = new ReactiveProperty<Vector3>(scale);
                InitialPosition = position;
                InitialScale = scale;
                
                _isMoveable = new ReactiveProperty<bool>(true);
                _isSelected = new ReactiveProperty<bool>(false);
                _interactable = new ReactiveProperty<bool>(false);
                _isMoving = new ReactiveProperty<bool>(false);
                _isVisible = new ReactiveProperty<bool>(true);
                
                IsMoveable = new ReadOnlyReactiveProperty<bool>(_isMoveable);
                IsSelected = new ReadOnlyReactiveProperty<bool>(_isSelected);
                Interactable = new ReadOnlyReactiveProperty<bool>(_interactable);
                IsMoving = new ReadOnlyReactiveProperty<bool>(_isMoving);
                Visible = new ReadOnlyReactiveProperty<bool>(_isVisible);
            }

            public void SetVisible(bool visible)
            {
                _isVisible.Value = visible;
            }

            public void SetPosition(Vector3 position)
            {
                _position.Value = position;
            }
              
            public void SetMoveable(bool isMoveable)
            {
                _isMoveable.Value = isMoveable;
            }

            public void SetSelected(bool selected)
            {
                _isSelected.Value = selected;
            }

            public void SetMoving(bool isMoving)
            {
                _isMoving.Value = isMoving;
            }
            
            public void SetInteractable(bool isSelectable)
            {
                _interactable.Value = isSelectable;
            }

            public void SetScale(Vector3 scale)
            {
                _scale.Value = scale;
            }

            public void Reset()
            {
                _scale.SetValueAndForceNotify(InitialScale);
                _position.SetValueAndForceNotify(InitialPosition);
                _isMoveable.Value = true;
                _isSelected.Value = false;
                _interactable.Value = false;
                _isMoving.Value = false;
                _isVisible.Value = true;
            }

            public void Dispose()
            {
                _position?.Dispose();
                _scale?.Dispose();
                _isMoveable?.Dispose();
                _isSelected?.Dispose();
                _interactable?.Dispose();
                _isMoving?.Dispose();
                _isVisible?.Dispose();
                Visible?.Dispose();
                IsMoveable?.Dispose();
                IsSelected?.Dispose();
                IsMoving?.Dispose();
                Interactable?.Dispose();
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