using System;
using UniRx;
using Zenject;

namespace Core.Common
{
    public interface IAppPreferencesProvider
    {
        AppPreferencesModel Current { get; }
    }
    public sealed class AppPreferencesProvider : IInitializable, IAppPreferencesProvider, IDisposable
    {
        private CompositeDisposable _disposables;
        private IRepository<AppPreferencesModel> _repository;
        public AppPreferencesModel Current { get; private set; }

        public AppPreferencesProvider(IRepository<AppPreferencesModel> repository)
        {
            _disposables = new CompositeDisposable();
            _repository = repository;
        }

        public void Initialize()
        {
            var model = _repository.Load();
            Current = model ?? CreateDefault();

            Observable
                .Merge(Current.Sound.Skip(1).DistinctUntilChanged().AsUnitObservable())
                .Merge(Current.Music.Skip(1).DistinctUntilChanged().AsUnitObservable())
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Subscribe(_ => ForceSave())
                .AddTo(_disposables);

        }

        private void ForceSave()
        {
            _repository.Save(Current);
        }

        private AppPreferencesModel CreateDefault()
        {
            var preferencesModel = new AppPreferencesModel();
            _repository.Save(preferencesModel);
            return preferencesModel;
        }
        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}