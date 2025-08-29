using System;
using Core.Data;
using Newtonsoft.Json;
using UniRx;
using Zenject;

namespace Core.User
{
    [Serializable]
    public class UserPreferencesModel
    {
        [JsonProperty] public UserModel User { get; private set; } = new();
        [JsonProperty] public ReactiveProperty<string> ProfileAssetId { get; private set; } = new();
        [JsonProperty] public ReactiveProperty<MaterialId> TileMaterialId { get; private set; } = new ();

        [JsonConstructor]
        public UserPreferencesModel()
        {
            
        }
        
        public UserPreferencesModel(UserModel userModel)
        {
            User = userModel;
            ProfileAssetId = new ReactiveProperty<string>("avatar_01");
            TileMaterialId = new ReactiveProperty<MaterialId>(MaterialId.Default);
        }
    }
}