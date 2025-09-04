using System;
using System.Collections.Generic;
using Game.Entities;
using Game.Field;
using UniRx;
using UnityEngine;

namespace Game.User
{
    public class UserEntitiesModel: IDisposable
    {
        public int Owner { get; }
        public ReactiveCollection<IPlaceableModel> Entities { get; private set; }
        private List<IPlaceableModel> _cache;
        
        public UserEntitiesModel(int owner, IEnumerable<IPlaceableModel> placeablePublishers)
        {
            Owner = owner;
            _cache = new List<IPlaceableModel>(placeablePublishers);
            Entities = new ReactiveCollection<IPlaceableModel>(placeablePublishers);
        }

        public UserEntitiesModel(int owner)
        {
            Owner = owner;
            _cache = new List<IPlaceableModel>();
            Entities = new ReactiveCollection<IPlaceableModel>();

            for (int i = 1; i <= 7; i++)
            {
                var model = new EntityModel(i, Owner, Vector3.zero);
                _cache.Add(model);
                Entities.Add(model);
            }
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

        public void Dispose()
        {
            Entities?.Dispose();
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