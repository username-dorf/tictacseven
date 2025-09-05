using Zenject;

namespace Core.Common
{
    public class CommonInstaller : Installer<CommonInstaller>
    {
        public override void InstallBindings()
        {
            BindSkinMaterialsContracts(Container);
            BindAppPreferences(Container);
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

        private void BindAppPreferences(DiContainer container)
        {
            container.BindInterfacesTo<AppPreferencesRepository>()
                .AsSingle();
            container.BindInterfacesTo<AppPreferencesProvider>()
                .AsSingle();
        }
    }
}