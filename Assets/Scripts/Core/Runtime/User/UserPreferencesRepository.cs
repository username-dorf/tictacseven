using Newtonsoft.Json;
using UnityEngine;

namespace Core.User
{
    public interface IUserPreferencesRepository
    {
        UserPreferencesModel Load();
        void Save(UserPreferencesModel model);
        void Delete();
    }
    public class PlayerPrefsUserPreferencesRepository : IUserPreferencesRepository
    {
        private const string PREFS_KEY = "com.tictacseven.user.preferences";
        
        public UserPreferencesModel Load()
        {
            var hasKey = PlayerPrefs.HasKey(PREFS_KEY);
            if (!hasKey)
                return null;
            var json = PlayerPrefs.GetString(PREFS_KEY, string.Empty);
            if (string.IsNullOrEmpty(json))
                return null;
            try
            {
                return JsonConvert.DeserializeObject<UserPreferencesModel>(json);
            }
            catch (JsonException e)
            {
                Debug.LogError($"Failed to deserialize UserPreferencesModel: {e.Message}");
                return null;
            }
        }

        public void Save(UserPreferencesModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            PlayerPrefs.SetString(PREFS_KEY, json);
        }

        public void Delete()
        {
            if (PlayerPrefs.HasKey(PREFS_KEY))
                PlayerPrefs.DeleteKey(PREFS_KEY);
        }
    }
}