using Newtonsoft.Json;
using UnityEngine;

namespace Core.Common
{
    public interface ISerializableModel
    {
        
    }
    public interface IRepository<T> where T : ISerializableModel
    {
        string Key { get; }
        
        public T Load()
        {
            var hasKey = PlayerPrefs.HasKey(Key);
            if (!hasKey)
                return default;
            var json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(json))
                return default;
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonException e)
            {
                Debug.LogError($"Failed to deserialize UserPreferencesModel: {e.Message}");
                return default;
            }
        }

        void Save(T model)
        {
            var json = JsonConvert.SerializeObject(model);
            PlayerPrefs.SetString(Key, json);
        }

        void Delete()
        {
            if (PlayerPrefs.HasKey(Key))
                PlayerPrefs.DeleteKey(Key);
        }
    }
}