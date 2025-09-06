using Core.VFX;
using Zenject;

namespace Game.Entities.VFX
{
    public class EntityPlacementFXPool : FxPoolService
    {
        public EntityPlacementFXPool(
            [Inject(Id = "FX_PlacePiece_Prefab")] PooledFXView prefab, 
            [InjectOptional(Id = "FX_Place_AutoRecycle")] float autoRecycleAfter = 1.6f,
            [InjectOptional(Id = "FX_Place_Prewarm")] int prewarm = 2,
            [InjectOptional(Id = "FX_Place_HardCap")] int hardCap = 0)
            : base(prefab, autoRecycleAfter, prewarm, hardCap)
        {
        }
    }
}