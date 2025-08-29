using Zenject;

namespace Core.User
{
    public class UserInstaller : Installer<UserInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ProfileSpriteSetsProvider>()
                .AsSingle();
            
            Container.BindInterfacesTo<PlayerPrefsUserPreferencesRepository>()
                .AsSingle();
            Container.BindInterfacesTo<UserPreferencesProvider>()
                .AsSingle();
            
            Container.Bind<NicknameFactory>()
                .AsSingle();
            Container.BindFactory<UserModel, UserModel.Factory>()
                .AsSingle();
        }
    }
}