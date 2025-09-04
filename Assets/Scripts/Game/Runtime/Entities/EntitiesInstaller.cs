using Zenject;

namespace Game.Entities
{
    public class EntitiesInstaller : Installer<EntitiesInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<EntitiesBackgroundFactory>()
                .AsSingle();
            Container.Bind<EntitiesBackgroundGridFactory>()
                .AsSingle();
            Container.Bind<EntitiesValueSpriteProvider>()
                .AsSingle();
            Container.Bind<EntitiesMaterialProvider>()
                .AsSingle();
            Container.Bind<EntityFactory>()
                .AsSingle();
        }
    }
}