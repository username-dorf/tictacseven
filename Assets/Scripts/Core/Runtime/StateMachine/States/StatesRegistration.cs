using System;
using Zenject;
using UniState;


namespace Core.StateMachine
{
    public class StatesRegistration : Installer<StatesRegistration>
    {
        public override void InstallBindings()
        {
            BindStates();
            BindStateTriggers();
        }
        private void BindStates()
        {
            Container.BindState<BootstrapState>();
            Container.BindState<PersistantResourcesLoadState>();
            Container.BindState<MenuState>();
            Container.BindState<GameState>();
            Container.BindState<MultiplayerGameState>();
        }
        public void BindStateTriggers()
        {
            Container.Bind<ManualTransitionTrigger<MenuState>>()
                .AsSingle();
            Container.Bind<ManualTransitionTrigger<GameState>>()
                .AsSingle();
            Container.Bind<ManualTransitionTrigger<MultiplayerGameState>>()
                .AsSingle();
        }
        
    }
}