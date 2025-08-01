using Zenject;

namespace Game.Field
{
    public class FieldInstaller : Installer<FieldInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<FieldViewFactory>()
                .AsSingle();
            Container.Bind<FieldGridFactory>()
                .AsSingle();
            Container.Bind<FieldViewProvider>()
                .AsSingle();
        }
    }
}