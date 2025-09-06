using Core.VFX;
using Zenject;

namespace Game.Entities.VFX
{
    public class GameVFXMonoInstaller : MonoInstaller
    {
        public PooledFXView fxPrefab;
        public override void InstallBindings()
        {
            Container
                .Bind<PooledFXView>()
                .WithId("FX_PlacePiece_Prefab")
                .FromInstance(fxPrefab)
                .WhenInjectedInto<FxPoolService>();
            
            
            Container.BindInterfacesAndSelfTo<EntityPlacementFXPool>()
                .AsTransient();
        }
    }
}