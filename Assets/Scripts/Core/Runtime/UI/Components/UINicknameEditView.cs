using System;
using Core.User;
using TMPro;
using UnityEngine;

namespace Core.UI.Components
{
    public class UINicknameEditView : MonoBehaviour
    {
        [SerializeField] public TMP_InputField inputField;

        public void Initialize(string nickname, Action<string> onNicknameChanged)
        {
            inputField.text = nickname;
            inputField.onEndEdit.AddListener(value=>onNicknameChanged?.Invoke(value));
        }
    }

    public class UINicknameEditPresenter
    {
        private UINicknameEditView _view;
        private IUserPreferencesProvider _userPreferencesProvider;

        public UINicknameEditPresenter(UINicknameEditView view,
            IUserPreferencesProvider userPreferencesProvider)
        {
            _userPreferencesProvider = userPreferencesProvider;
            _view = view;
        }

        public void Initialize()
        {
            var current = _userPreferencesProvider.Current.User.Nickname.Value;
            _view.Initialize(current,OnEndEdit);
        }

        private void OnEndEdit(string nickname)
        {
            _userPreferencesProvider.Current.User.Nickname.Value = nickname;
        }
    }
}