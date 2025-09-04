using System;
using System.Collections.Generic;
using Game.Entities;
using Game.Field;
using Game.User;
using Zenject;

namespace Game.States
{
    public interface IGameSubstatesInstaller
    {
        IGameSubstatesInstaller BindFieldModel(FieldModel fieldModel);
        IGameSubstatesInstaller BindEntitiesModel(UserEntitiesModel model, object identifier);
        IGameSubstatesInstaller BindPlaceholderPresenter(
            EntitiesBackgroundView.EntitiesPlaceholderPresenter placeholderPresenter,
            object identifier);
        IGameSubstatesInstaller BindUserRoundModel(UserRoundModel roundModel, object identifier);
        void Build();
    }

    public interface IGameSubstateResolver
    {
        T Resolve<T>();
    }
    public sealed class GameSubstatesFacade :
        IGameSubstatesInstaller, IGameSubstateResolver, IDisposable
    {
        public const string ROUND_MODELS_ALIAS = "Players";
        private readonly DiContainer _sub;
        private bool _built;

        public GameSubstatesFacade(DiContainer sub) => _sub = sub;


        public IGameSubstatesInstaller BindFieldModel(FieldModel fieldModel)
        {
            _sub.BindInterfacesAndSelfTo<FieldModel>().FromInstance(fieldModel);
            return this;
        }

        public IGameSubstatesInstaller BindEntitiesModel(UserEntitiesModel model, object identifier)
        {
            _sub.Bind<UserEntitiesModel>().WithId(identifier).FromInstance(model);
            _sub.BindInterfacesTo<UserEntitiesModel>().FromInstance(model);
            return this;
        }

        public IGameSubstatesInstaller BindPlaceholderPresenter(
            EntitiesBackgroundView.EntitiesPlaceholderPresenter placeholderPresenter,
            object identifier)
        {
            _sub.Bind<EntitiesBackgroundView.EntitiesPlaceholderPresenter>()
                .WithId(identifier).FromInstance(placeholderPresenter);
            return this;
        }

        public IGameSubstatesInstaller BindUserRoundModel(UserRoundModel roundModel, object identifier)
        {
            _sub.Bind<UserRoundModel>().WithId(identifier).FromInstance(roundModel);

            _sub.Bind<UserRoundModel>()
                .WithId(ROUND_MODELS_ALIAS)
                .FromInstance(roundModel)
                .AsCached();

            _sub.BindInterfacesTo<UserRoundModel>().FromInstance(roundModel);
            return this;
        }

        public void Build()
        {
            if (_built) return;
            _built = true;

            _sub.BindInterfacesTo<ActiveUserProvider>().AsSingle();
        }


        public T Resolve<T>() => _sub.Resolve<T>();

        public IEnumerable<T> ResolveAll<T>() => _sub.ResolveAll<T>();


        public void Dispose()
        {
        }
    }
}