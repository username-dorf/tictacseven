using UnityEngine;

namespace Core.User
{
    public class NicknameFactory
    {
        public string Create()
        {
            return "User_" + Random.Range(1000, 9999).ToString();
        }
    }
}