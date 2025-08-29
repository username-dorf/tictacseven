using Core.UI.Components;
using Game.Entities;
using Game.Field;
using Game.States;
using Game.UI;
using Game.User;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameMonoInstaller : MonoInstaller
    {
        [SerializeField] private UIProvider<UIGame> uiGame; 

        public override void InstallBindings()
        {
            FieldInstaller.Install(Container);
            EntitiesInstaller.Install(Container);
            AgentAIInstaller.Install(Container);
            
            Container.BindInterfacesTo<GameBootstrap>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<GameSubstateInstaller>()
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
            
            Container.Bind<UIProvider<UIGame>>()
                .FromInstance(uiGame)
                .AsSingle();
            Container.BindInterfacesTo<UIGameController>()
                .AsSingle();
        }
    }
}