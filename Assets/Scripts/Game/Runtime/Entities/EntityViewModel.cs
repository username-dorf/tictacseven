using System;
using UniRx;
using UnityEngine;

namespace Game.Entities
{
    public class EntityViewModel: IDisposable
    {
        public ReadOnlyReactiveProperty<int> Value { get; }
        public ReactiveProperty<Material> Material { get; }
        public ReactiveProperty<Sprite> ValueSprite { get; }
        public ReadOnlyReactiveProperty<bool> IsMoving => 
            _model.Transform.IsMoving;
        public ReadOnlyReactiveProperty<Vector3> Position => 
            _model.Transform.Position;
        public ReadOnlyReactiveProperty<Vector3> Scale => 
            _model.Transform.Scale;
        public ReadOnlyReactiveProperty<bool> IsVisible =>
            _model.Transform.Visible;

        public ReadOnlyReactiveProperty<bool> IsInteractable =>
            _model.Transform.Interactable;
        
        private CompositeDisposable _disposable;
        private EntityModel _model;

        public EntityViewModel(EntityModel model, Sprite valueSprite, Material material)
        {
            _model = model;
            _disposable = new CompositeDisposable();
            
            Value = new ReadOnlyReactiveProperty<int>(model.Data.Merit); //debug purpose
            ValueSprite = new ReactiveProperty<Sprite>(valueSprite);
            Material = new ReactiveProperty<Material>(material);
            
            _model.Transform.IsSelected
                .Where(x => !x)
                .Skip(1)
                .Subscribe(_ => OnRelease())
                .AddTo(_disposable);
        }

        public void SetSelected(bool isSelected)
        {
            if(isSelected && !CanBeMoved())
                return; 
            
            if(!isSelected && !_model.Transform.IsSelected.Value)
                return;
            
            _model.Transform.SetSelected(isSelected);
        }
        public void SetMoving(bool isDragging)
        {
            if(isDragging && !CanBeMoved())
                return;
            
            _model.Transform.SetMoving(isDragging);
        }
        public void SetPosition(Vector3 position)
        {
            if(!CanBeMoved())
                return;
            
            _model.Transform.SetPosition(position);
        }

        private bool CanBeMoved()
        {
            return _model.Transform.IsMoveable.Value;
        }

        private void OnRelease()
        {
            _model.Events.ReleaseCommand.Execute(_model);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _model.Dispose();
            Value?.Dispose();
            IsMoving?.Dispose();
        }
    }
}