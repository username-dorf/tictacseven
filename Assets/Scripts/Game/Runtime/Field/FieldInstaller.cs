using Zenject;

namespace Game.Field
{
    public class FieldInstaller : Installer<FieldInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<FieldFactory>()
                .AsSingle();
            Container.Bind<FieldGridFactory>()
                .AsSingle();
        }
    }
}