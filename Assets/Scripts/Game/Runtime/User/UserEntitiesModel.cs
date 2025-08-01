using System;
using System.Collections.Generic;
using Game.Field;
using UniRx;

namespace Game.User
{
    public class UserEntitiesModel
    {
        public ReactiveCollection<IPlaceableModel> Entities { get; }
        
        public UserEntitiesModel(IEnumerable<IPlaceableModel> placeablePublishers)
        {
            Entities = new ReactiveCollection<IPlaceableModel>(placeablePublishers);
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