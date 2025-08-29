using System.Linq;
using Core.AssetProvider;
using UnityEngine;

namespace Core.User
{
    public sealed class ProfileSpriteSetsProvider
        : AssetsProvider<ProfileSpriteScriptableObject, string>
    {
        public ProfileSpriteSetsProvider()
            : base(a => a.Id, k => k?.Trim(), returnSingleOnMiss: false)
        {
        }

        public string GetRandomSet()
        {
            var maxLength = _assets.Count;
            var randomIndex = Random.Range(0, maxLength);
            return _assets.ElementAt(randomIndex).Key;
        }
    }
}