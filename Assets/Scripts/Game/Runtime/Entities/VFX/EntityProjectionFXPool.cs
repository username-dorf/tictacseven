using Core.VFX;
using Zenject;

namespace Game.Entities.VFX
{
    public class EntityProjectionFXPool: WorldFxPoolService
    {
        public EntityProjectionFXPool(
            [InjectOptional(Id = "WorldFX_AutoRecycle")] float autoRecycleAfter,
            [InjectOptional(Id = "WorldFX_Prewarm")] int prewarm,
            [InjectOptional(Id = "WorldFX_HardCap")] int hardCap) : base(autoRecycleAfter, prewarm, hardCap)
        {
        }
    }
}