using Core.StateMachine;
using Game.Entities;
using Game.Field;
using Game.States;
using Game.UI;
using Game.User;
using UnityEngine.Scripting;
using Zenject;

namespace Game
{
    [Preserve] 
    public class GameMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            FieldInstaller.Install(Container);
            EntitiesInstaller.Install(Container);
            AgentAIInstaller.Install(Container);
            
            Container.BindInterfacesTo<GameBootstrap>()
                .AsSingle();
            Container.BindExecutionOrder<GameBootstrap>(100);

            Container.Bind(typeof(IGameSubstatesInstaller), typeof(IGameSubstateResolver))
                .FromSubContainerResolve()
                .ByNewGameObjectMethod(InstallSubcontainer)
                .AsSingle();

            Container.BindInterfacesTo<GameUIService>()
                .AsSingle();
           
            
            
            Container.BindFactory<UserRoundModel,UserRoundModel.Factory>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<UserRoundModel.Provider>()
                .AsSingle();
            
            Container.BindFactory<AIUserRoundModel,AIUserRoundModel.Factory>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<AIUserRoundModel.Provider>()
                .AsSingle();

            
            Container.BindInterfacesTo<UIGameController>()
                .AsSingle();
        }

        private void InstallSubcontainer(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<StateFactory>()
                .AsSingle();
            
            container.BindInterfacesTo<StateMachine>()
                .AsSingle();

            container.InstallState<InitialSubstate>();
            container.InstallState<UserMoveSubstate>();
            container.InstallState<AgentAIMoveSubstate>();
            container.InstallState<ValidateSubstate>();
            container.InstallState<RoundResultSubstate>();
            container.InstallState<RoundClearSubstate>();
            container.InstallState<FinalRoundResultSubstateGameSubstate>();
            
            container.BindInterfacesTo<GameSubstatesFacade>()
                .AsSingle();
        }
    }
}