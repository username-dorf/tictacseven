using System;
using System.Collections.Generic;
using Core.StateMachine;
using Game.Entities;
using Game.Field;
using Game.User;
using Zenject;

namespace Game.States
{
    public interface IGameSubstateResolver: IDisposable
    {
        public T Resolve<T>();
    }

    public interface IGameSubstatesInstaller
    {
        GameSubstateInstaller BindFieldModel(FieldModel fieldModel);
        GameSubstateInstaller BindEntitiesModel(UserEntitiesModel model, object identifier);
        GameSubstateInstaller BindPlaceholderPresenter(
            EntitiesBackgroundView.EntitiesPlaceholderPresenter placeholderPresenter,
            object identifier);
        GameSubstateInstaller BindUserRoundModel(UserRoundModel roundModel, object identifier);
        void Build();
    }

    public class GameSubstateSettings
    {
        public const string ROUND_MODELS_ALIAS = "Players";
    }
    public class GameSubstateInstaller : IGameSubstateResolver,IGameSubstatesInstaller
    {
        private DiContainer _subContainer;
        private DiContainer _container;
        private SubInstaller _installer;

        public GameSubstateInstaller(DiContainer container)
        {
            _container = container;
            _subContainer = _container.CreateSubContainer();

            _installer = new SubInstaller();
            _installer.InstallBindings(_subContainer);
        }

        public GameSubstateInstaller BindFieldModel(FieldModel fieldModel)
        {
            _subContainer
                .BindInterfacesAndSelfTo<FieldModel>()
                .FromInstance(fieldModel);
            return this;
        }
        
        public GameSubstateInstaller BindEntitiesModel(UserEntitiesModel model, object identifier)
        {
            _subContainer
                .Bind<UserEntitiesModel>()
                .WithId(identifier)
                .FromInstance(model);
            _subContainer.BindInterfacesTo<UserEntitiesModel>()
                .FromInstance(model);
            return this;
        }
        
        public GameSubstateInstaller BindPlaceholderPresenter(EntitiesBackgroundView.EntitiesPlaceholderPresenter placeholderPresenter,
            object identifier)
        {
            _subContainer
                .Bind<EntitiesBackgroundView.EntitiesPlaceholderPresenter>()
                .WithId(identifier)
                .FromInstance(placeholderPresenter);
            return this;
        }

        public GameSubstateInstaller BindUserRoundModel(UserRoundModel roundModel, object identifier)
        {
            _subContainer
                .Bind<UserRoundModel>()
                .WithId(identifier)
                .FromInstance(roundModel);
            
            //alias
            _subContainer.Bind<UserRoundModel>().WithId(GameSubstateSettings.ROUND_MODELS_ALIAS)
                .FromInstance(roundModel)
                .AsCached();

            _subContainer.BindInterfacesTo<UserRoundModel>()
                .FromInstance(roundModel);
            
            return this;
        }

        public void Build()
        {
            _subContainer.BindInterfacesTo<ActiveUserProvider>()
                .AsSingle();
        }

        public T Resolve<T>()
        {
            return _subContainer.Resolve<T>();
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return _subContainer.ResolveAll<T>();
        }

        private class SubInstaller
        {
            public void InstallBindings(DiContainer container)
            {
                container
                    .Bind<StateFactory>()
                    .To<StateFactory>()
                    .AsSingle();

                container
                    .Bind<IStateMachine>()
                    .To<StateMachine>()
                    .AsSingle();

                container.InstallState<InitialSubstate>();
                container.InstallState<UserMoveSubstate>();
                container.InstallState<AgentAIMoveSubstate>();
                container.InstallState<ValidateSubstate>();
                container.InstallState<RoundResultSubstate>();
                container.InstallState<RoundClearSubstate>();
                container.InstallState<FinalRoundResultSubstateGameSubstate>();
                
            }
        }

        public void Dispose()
        {
            var disp = _subContainer.TryResolve<DisposableManager>();
            disp?.Dispose();
        }
    }
}