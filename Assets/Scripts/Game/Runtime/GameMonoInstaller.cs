using Game.Entities;
using Game.Field;
using Game.States;
using Game.User;
using Zenject;

namespace Game
{
    public class GameMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            FieldInstaller.Install(Container);
            EntitiesInstaller.Install(Container);
            AgentAIInstaller.Install(Container);
            
            Container.BindInterfacesTo<GameBootstrap>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<GameSubstateInstaller>()
                .AsSingle();
           
            
            
            Container.BindFactory<UserRoundModel,UserRoundModel.Factory>()
                .AsSingle();
            Container.Bind<UserRoundModel.Provider>()
                .AsSingle();
            
            Container.BindFactory<AIUserRoundModel,AIUserRoundModel.Factory>()
                .AsSingle();
            Container.Bind<AIUserRoundModel.Provider>()
                .AsSingle();
        }
    }
}