using System;
using Core.Data;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using UniRx;

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

    [Serializable]
    public struct UserPreferencesDto : INetSerializable
    {
        public string id;
        public string nickname;
        public string profileAssetId;
        public int tileMaterialId;
        
        public UserPreferencesDto(string id, string nickname, string profileAssetId, MaterialId tileMaterialId)
        {
            this.id = id;
            this.nickname = nickname;
            this.profileAssetId = profileAssetId;
            this.tileMaterialId = (int) tileMaterialId;
        }

        public static UserPreferencesDto Create(UserPreferencesModel model)
        {
            return new UserPreferencesDto(model.User.Id, model.User.Nickname.Value, model.ProfileAssetId.Value,
                model.TileMaterialId.Value);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(nickname);
            writer.Put(profileAssetId);
            writer.Put(tileMaterialId);
        }

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetString();
            nickname = reader.GetString();
            profileAssetId = reader.GetString();
            tileMaterialId = reader.GetInt();
        }
    }
}