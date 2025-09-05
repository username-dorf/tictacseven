using Newtonsoft.Json;
using UniRx;

namespace Core.Common
{
    public sealed class AppPreferencesModel : ISerializableModel
    {
        [JsonProperty] public ReactiveProperty<bool> Sound { get; private set; } = new ReactiveProperty<bool>(true);
        [JsonProperty] public ReactiveProperty<bool> Music { get; private set; } = new ReactiveProperty<bool>(true);

    }

    public class AppPreferencesRepository : IRepository<AppPreferencesModel>
    {
        public string Key => "com.tictacseven.app.preferences";
    }
}