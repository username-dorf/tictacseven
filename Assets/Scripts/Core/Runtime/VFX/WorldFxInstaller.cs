using Core.VFX;
using UnityEngine;
using Zenject;

public class WorldFxInstaller : Installer<WorldFxInstaller>
{
    private float _autoRecycleAfter = 8.0f;
    private int _prewarm = 10;
    private int _hardCap = 50;

    public override void InstallBindings()
    {
        Container.Bind<float>()
            .WithId("WorldFX_AutoRecycle")
            .FromInstance(_autoRecycleAfter);
      
        Container.Bind<int>()
            .WithId("WorldFX_Prewarm")
            .FromInstance(_prewarm);
        
        Container.Bind<int>()
            .WithId("WorldFX_HardCap")
            .FromInstance(_hardCap);
        
    }
}