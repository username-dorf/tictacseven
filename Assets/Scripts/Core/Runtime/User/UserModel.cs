using UnityEditor;
using Zenject;

namespace Core.User
{
    public class UserModel
    {
        public GUID Id { get; }
        public string Nickname { get; }

        [Inject]
        public UserModel(NicknameFactory nicknameFactory)
        {
            Nickname = nicknameFactory.Create();
            Id = GUID.Generate();
        }
        
        public UserModel(string nickname)
        {
            Nickname = nickname;
            Id = GUID.Generate();
        }
        
        public class Factory : PlaceholderFactory<UserModel>
        {
            
        }
    }
}