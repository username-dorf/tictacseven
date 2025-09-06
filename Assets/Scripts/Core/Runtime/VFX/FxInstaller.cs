using UnityEngine;
using Zenject;

namespace Core.VFX
{
    public class FxInstaller : Installer<FxInstaller>
    {
        private float _autoRecycleAfter = 1.6f;
        private int _prewarm = 2;
        private int _hardCap = 8;

        public override void InstallBindings()
        {
            
            Container.Bind<float>()
                .WithId("FX_Place_AutoRecycle")
                .FromInstance(_autoRecycleAfter);
          
            Container.Bind<int>()
                .WithId("FX_Place_Prewarm")
                .FromInstance(_prewarm);
            
            Container.Bind<int>()
                .WithId("FX_Place_HardCap")
                .FromInstance(_hardCap);

        }
    }
}