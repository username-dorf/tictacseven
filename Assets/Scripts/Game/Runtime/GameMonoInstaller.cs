using Game.Field;
using Zenject;

namespace Game
{
    public class GameMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            FieldInstaller.Install(Container);
            
            Container.BindInterfacesTo<GameBootstrap>()
                .AsSingle();
        }
    }
}