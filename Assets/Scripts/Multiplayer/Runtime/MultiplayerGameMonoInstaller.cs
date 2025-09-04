using Core.StateMachine;
using Core.UI.Components;
using Game.Entities;
using Game.Field;
using Game.States;
using Game.UI;
using Game.User;
using Multiplayer.Client;
using Multiplayer.Client.States;
using Multiplayer.UI;
using UnityEngine;
using Zenject;
using InitialSubstate = Multiplayer.Client.States.InitialSubstate;
using RoundClearSubstate = Multiplayer.Client.States.RoundClearSubstate;
using RoundResultSubstate = Multiplayer.Client.States.RoundResultSubstate;

namespace Multiplayer
{
    public class MultiplayerGameMonoInstaller : MonoInstaller
    {
        [SerializeField] private UIProvider<UIGame> uiGame; 
        public override void InstallBindings()
        {
            FieldInstaller.Install(Container);
            EntitiesInstaller.Install(Container);

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
            
            
            Container.Bind<UIProvider<UIGame>>()
                .FromInstance(uiGame)
                .AsSingle();
            Container.BindInterfacesTo<UIMultiplayerController>()
                .AsSingle();
        }
        void InstallSubcontainer(DiContainer sub)
        {
            sub.BindInterfacesAndSelfTo<StateFactory>()
                .AsSingle();
            
            sub.BindInterfacesTo<StateMachine>()
                .AsSingle();
            
            sub.InstallState<InitialSubstate>();
            sub.InstallState<TurnSubstate>();
            sub.InstallState<ServerSyncSubstate>();
            sub.InstallState<RoundResultSubstate>();
            sub.InstallState<RoundClearSubstate>();

            sub.BindInterfacesTo<GameSubstatesFacade>()
                .AsSingle();

            sub.BindInterfacesTo<ClientSubstateController>()
                .AsSingle()
                .NonLazy();
        }
    }
}