using Zenject;

namespace Core.User
{
    public class UserInstaller : Installer<UserInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<NicknameFactory>()
                .AsSingle();
            Container.BindFactory<UserModel, UserModel.Factory>()
                .AsSingle();
            
            Container.Bind<UserModel>()
                .AsCached();
            
            
        }
    }
}