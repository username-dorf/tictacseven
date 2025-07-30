using System;
using Game.Field;
using UniRx;
using UnityEngine;

namespace Game.Entities
{
   
    public interface IEntityReleasePublisher : IReleasePublisher<IPlaceableModel>
    {
    }
    
    public class EntityViewModel: IEntityReleasePublisher, IDisposable
    {
        public ReadOnlyReactiveProperty<int> Value { get; }
        public ReactiveProperty<bool> IsDragging { get; }
        public ReactiveProperty<bool> IsSelected { get; }
        
        public ReadOnlyReactiveProperty<Vector3> Position => 
            _model.Transform.Position.ToReadOnlyReactiveProperty();
        public IObservable<IPlaceableModel> ReleaseCommand => _releaseCommand;


        public Plane? DragPlane
        {
            get
            {
                if (!_fieldViewProvider.Initialized)
                    return null;
                return _fieldViewProvider.View.DragPlane;
            }
        }

        private CompositeDisposable _disposable;
        private FieldViewProvider _fieldViewProvider;
        private EntityModel _model;
        private ReactiveCommand<IPlaceableModel> _releaseCommand;

        public EntityViewModel(EntityModel model, FieldViewProvider fieldViewProvider)
        {
            _model = model;
            _fieldViewProvider = fieldViewProvider;
            _disposable = new CompositeDisposable();
            
            IsSelected = new ReactiveProperty<bool>(false);
            IsDragging = new ReactiveProperty<bool>(false);
            
            
            Value = new ReadOnlyReactiveProperty<int>(model.Value);
            _releaseCommand = new ReactiveCommand<IPlaceableModel>();
            
            IsSelected
                .Where(x => !x)
                .Skip(1)
                .Subscribe(_ => OnRelease())
                .AddTo(_disposable);
        }

        public void SetSelected(bool isSelected)
        {
            IsSelected.Value = isSelected;
        }
        public void SetDragging(bool isDragging)
        {
            IsDragging.Value = isDragging;
        }
        public void SetPosition(Vector3 position)
        {
            _model.Transform.Position.Value = position;
        }

        private void OnRelease()
        {
            Debug.Log($"EntityViewModel: OnRelease called {_model.Transform.Position.Value}");
            _releaseCommand.Execute(_model);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _model.Dispose();
            Value?.Dispose();
            IsDragging?.Dispose();
            IsSelected?.Dispose();
            _releaseCommand?.Dispose();
        }
    }
}