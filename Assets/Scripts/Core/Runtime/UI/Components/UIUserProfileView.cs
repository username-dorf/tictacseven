using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace Core.UI.Components
{
    public class UIUserProfileView : MonoBehaviour
    {
        [SerializeField] private UIProfileView profileView;
        [SerializeField] private TextMeshProUGUI nicknameText;

        public void Initialize(IObservable<Sprite> profileSprite, IObservable<string> nicknameText)
        {
            profileView.Initialize(profileSprite);
            nicknameText
                .Subscribe(SetNickname)
                .AddTo(this);
        }

        private void SetNickname(string nickname)
        {
            if (nicknameText != null)
                nicknameText.text = nickname;
        }
    }
}