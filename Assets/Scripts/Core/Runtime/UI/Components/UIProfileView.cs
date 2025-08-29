using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public class UIProfileView : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;

        public void Initialize(IObservable<Sprite> profileSprite)
        {
            profileSprite
                .Subscribe(SetProfileSprite)
                .AddTo(this);
        }
        
        private void SetProfileSprite(Sprite sprite)
        {
            if(avatarImage != null)
                avatarImage.sprite = sprite;
        }
    }
}