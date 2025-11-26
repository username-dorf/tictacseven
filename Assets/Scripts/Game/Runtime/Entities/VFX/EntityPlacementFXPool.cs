using Core.VFX;
using Zenject;

namespace Game.Entities.VFX
{
    public class EntityPlacementFXPool : FxPoolService
    {
        public EntityPlacementFXPool(
            [Inject(Id = "FX_PlacePiece_Prefab")] PooledFXView prefab, 
            [InjectOptional(Id = "FX_Place_AutoRecycle")] float autoRecycleAfter,
            [InjectOptional(Id = "FX_Place_Prewarm")] int prewarm,
            [InjectOptional(Id = "FX_Place_HardCap")] int hardCap)
            : base(prefab, autoRecycleAfter, prewarm, hardCap)
        {
        }
    }
}