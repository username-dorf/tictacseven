using Zenject;

namespace Game.Field
{
    public class FieldInstaller : Installer<FieldInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<FieldViewFactory>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<FieldGridFactory>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<FieldViewProvider>()
                .AsSingle();
        }
    }
}