using Core.Common;

namespace Core.User
{
    public class PlayerPrefsUserPreferencesRepository : IRepository<UserPreferencesModel>
    {
        public string Key => "com.tictacseven.user.preferences";
    }
}