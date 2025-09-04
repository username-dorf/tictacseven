using System;
using System.Collections.Generic;
using Game.Field;
using UniRx;

namespace Game.User
{
    public class UserEntitiesModel
    {
        public ReactiveCollection<IPlaceableModel> Entities { get; private set; }
        private List<IPlaceableModel> _cache;
        
        public UserEntitiesModel(IEnumerable<IPlaceableModel> placeablePublishers)
        {
            _cache = new List<IPlaceableModel>(placeablePublishers);
            Entities = new ReactiveCollection<IPlaceableModel>(placeablePublishers);
        }

        public UserEntitiesModel SetInteractionAll(bool value)
        {
            foreach (var placeableModel in Entities)
            {
                placeableModel.Transform.SetInteractable(value);
            }
            return this;
        }
        public void Drop()
        {
            foreach (var placeableModel in _cache)
            {
                placeableModel.Transform.Reset();
            }
            Entities.Clear();
            Entities = new ReactiveCollection<IPlaceableModel>(_cache);
        }
    }

    public class UserEntitiesViewModel: IDisposable
    {
        private CompositeDisposable _disposables;
        
        public UserEntitiesViewModel(UserEntitiesModel model)
        {
            _disposables = new CompositeDisposable();
            foreach (var placeableModel in model.Entities)
            {
                placeableModel.Events.ReleaseApprovedCommand
                    .Subscribe(placeable => OnReleaseApproved(model, placeable))
                    .AddTo(_disposables);
            }
        }
        private void OnReleaseApproved(UserEntitiesModel model, IPlaceableModel placeable)
        {
           model.Entities.Remove(placeable);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}