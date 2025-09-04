using System;
using Newtonsoft.Json;
using UniRx;
using Zenject;

namespace Core.User
{
    [Serializable]
    public class UserModel : IDisposable
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public ReactiveProperty<string> Nickname { get; private set; } = new();
        [JsonIgnore] public IObservable<string> OnIdChanged => _onIdChanged;
        [JsonIgnore] private Subject<string> _onIdChanged = new();

        [JsonConstructor]
        public UserModel()
        {

        }
        
        [Inject]
        public UserModel(NicknameFactory nicknameFactory)
        {
            Nickname = new ReactiveProperty<string>(nicknameFactory.Create());
            Id = Guid.NewGuid().ToString();
        }
        
        public UserModel(string id, string nickname)
        {
            Nickname = new ReactiveProperty<string>(nickname);
            Id = id;
        }
        
        #if UNITY_EDITOR
        public void ChangeId(string id)
        {
            Id = id;
            _onIdChanged?.OnNext(Id);
        }
        #endif
        
        public class Factory : PlaceholderFactory<UserModel>
        {
            
        }

        public void Dispose()
        {
            _onIdChanged?.Dispose();
            Nickname?.Dispose();
        }
    }
}