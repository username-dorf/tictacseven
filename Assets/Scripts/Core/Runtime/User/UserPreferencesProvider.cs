using System;
using Core.Common;
using UniRx;
using Zenject;

namespace Core.User
{
    public interface IUserPreferencesProvider
    {
        UserPreferencesModel Current { get; }
    }
    public class UserPreferencesProvider : IInitializable, IUserPreferencesProvider, IDisposable
    {
        private CompositeDisposable _disposables;
        private IRepository<UserPreferencesModel> _repository;
        private UserModel.Factory _userModelFactory;
        public UserPreferencesModel Current { get; private set; }

        public UserPreferencesProvider(UserModel.Factory userModelFactory, IRepository<UserPreferencesModel> repository)
        {
            _disposables = new CompositeDisposable();
            _userModelFactory = userModelFactory;
            _repository = repository;
        }

        public void Initialize()
        {
            var model = _repository.Load();
            Current = model ?? CreateDefault();

            Observable
                .Merge(Current.ProfileAssetId.Skip(1).DistinctUntilChanged().AsUnitObservable())
                .Merge(Current.TileMaterialId.Skip(1).DistinctUntilChanged().AsUnitObservable())
                .Merge(Current.User.OnIdChanged.DistinctUntilChanged().AsUnitObservable())
                .Merge(Current.User.Nickname.Skip(1).DistinctUntilChanged().AsUnitObservable())
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Subscribe(_ => ForceSave())
                .AddTo(_disposables);

        }

        private void ForceSave()
        {
            _repository.Save(Current);
        }

        private UserPreferencesModel CreateDefault()
        {
            var userModel = _userModelFactory.Create();
            var preferencesModel = new UserPreferencesModel(userModel);
            _repository.Save(preferencesModel);
            return preferencesModel;
        }
        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}