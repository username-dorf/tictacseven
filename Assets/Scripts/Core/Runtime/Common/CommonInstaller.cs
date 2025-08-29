using Zenject;

namespace Core.Common
{
    public class CommonInstaller : Installer<CommonInstaller>
    {
        public override void InstallBindings()
        {
            BindSkinMaterialsContracts(Container);
        }

        private void BindSkinMaterialsContracts(DiContainer container)
        {
            container.BindInterfacesTo<SkinMaterialMapProvider>()
                .AsSingle();
            container.Bind<SkinMaterialAssetsProvider>()
                .AsCached();
            container.Bind<SkinPreviewCamera.Factory>()
                .AsSingle();
        }
    }
}