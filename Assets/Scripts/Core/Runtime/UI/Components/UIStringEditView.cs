using System;
using Core.User;
using TMPro;
using UnityEngine;

namespace Core.UI.Components
{
    public class UIStringEditView : MonoBehaviour
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
        private UIStringEditView _view;
        private IUserPreferencesProvider _userPreferencesProvider;

        public UINicknameEditPresenter(UIStringEditView view,
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
    public class UIIdEditPresenter
    {
        private UIStringEditView _view;
        private IUserPreferencesProvider _userPreferencesProvider;

        public UIIdEditPresenter(UIStringEditView view,
            IUserPreferencesProvider userPreferencesProvider)
        {
            _userPreferencesProvider = userPreferencesProvider;
            _view = view;
        }

        public void Initialize()
        {
            var current = _userPreferencesProvider.Current.User.Id;
            _view.Initialize(current,OnEndEdit);
        }

        public void SetVisible(bool isVisible)
        {
            _view.gameObject.SetActive(isVisible);
        }

        private void OnEndEdit(string id)
        {
            #if UNITY_EDITOR
            _userPreferencesProvider.Current.User.ChangeId(id);
            #endif
        }
    }
}