using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.User
{
    public enum ProfileEmotion
    {
        Default,
        Sad,
        Happy
    }
    [CreateAssetMenu(menuName = "Assets/Profile Sprite Set", fileName = "ProfileSpriteSet")]
    public class ProfileSpriteScriptableObject : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public List<EmotionSprite> Emotions { get; private set; }

        public Sprite GetEmotionSprite(ProfileEmotion emotion)
        {
            return Emotions.FirstOrDefault(x=>x.Value == emotion)?.Sprite;
        }
        
        [Serializable]
        public class EmotionSprite
        {
            [field: SerializeField] public ProfileEmotion Value { get; private set; }
            [field: SerializeField] public Sprite Sprite { get; private set; }
        }
    }
}