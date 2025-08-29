using System;
using Newtonsoft.Json;
using UniRx;
using UnityEditor;
using Zenject;

namespace Core.User
{
    [Serializable]
    public class UserModel
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public ReactiveProperty<string> Nickname { get; private set; } = new();

        [JsonConstructor]
        public UserModel()
        {

        }
        
        [Inject]
        public UserModel(NicknameFactory nicknameFactory)
        {
            Nickname = new ReactiveProperty<string>(nicknameFactory.Create());
            Id = GUID.Generate().ToString();
        }
        
        public UserModel(string nickname)
        {
            Nickname = new ReactiveProperty<string>(nickname);
            Id = GUID.Generate().ToString();
        }
        
        public class Factory : PlaceholderFactory<UserModel>
        {
            
        }
    }
}