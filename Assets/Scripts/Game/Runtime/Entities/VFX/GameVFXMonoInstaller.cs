using Core.VFX;
using Zenject;

namespace Game.Entities.VFX
{
    public class GameVFXMonoInstaller : MonoInstaller
    {
        public PooledFXView placeFxPrefab;
        public PooledFXView destroyFxPrefab;
        public PooledFXView projectionFxPrefab;
        
        public override void InstallBindings()
        {
            BindPlaceFX();
            BindDestroyFX();
            BindProjectionFX();
        }
        private void BindDestroyFX()
        {
            var fxPool = Container.Instantiate<EntityDestroyFXPool>();
            Container.BindInterfacesAndSelfTo<EntityDestroyFXPool>()
                .FromInstance(fxPool);
            fxPool.RegisterPrefab("destroy", destroyFxPrefab);
            fxPool.Initialize();
        }

        private void BindPlaceFX()
        {
            Container
                .Bind<PooledFXView>()
                .WithId("FX_PlacePiece_Prefab")
                .FromInstance(placeFxPrefab)
                .WhenInjectedInto<FxPoolService>();
            
            
            Container.BindInterfacesAndSelfTo<EntityPlacementFXPool>()
                .AsTransient();
        }
        
        private void BindProjectionFX()
        {
            var fxPool = Container.Instantiate<EntityProjectionFXPool>();
            Container.BindInterfacesAndSelfTo<EntityProjectionFXPool>()
                .FromInstance(fxPool);
            fxPool.RegisterPrefab("projection", projectionFxPrefab);
            fxPool.Initialize();
        }
    }
}