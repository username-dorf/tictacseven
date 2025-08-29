using Core.Common;
using Zenject;

namespace Game.Entities
{
    public class EntitiesInstaller : Installer<EntitiesInstaller>
    {
        public override void InstallBindings()
        {
            Container.Decorate<ISkinMaterialMapProvider>()
                .With<GameSkinMaterialMapProvider>();
            
            Container.Bind<EntitiesBackgroundFactory>()
                .AsSingle();
            Container.Bind<EntitiesBackgroundGridFactory>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<EntitiesValueSpriteProvider>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<EntitiesMaterialAssetsProvider>()
                .AsTransient();
            Container.BindInterfacesAndSelfTo<EntityFactory>()
                .AsSingle();
        }
    }
}