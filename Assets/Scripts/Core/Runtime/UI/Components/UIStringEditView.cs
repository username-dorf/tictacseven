using System;
using Core.User;
using TMPro;
using UniRx;
using UnityEngine;

namespace Core.UI.Components
{
    public class UIStringEditView : MonoBehaviour
    {
        [SerializeField] public TMP_InputField inputField;

        public void Initialize(string inputValue, Action<string> onEndEdit)
        {
            inputField.text = inputValue;
            inputField.onEndEdit.AddListener(value=>onEndEdit?.Invoke(value));

        }
        public void Initialize(string inputValue, IObservable<string> onChangeNickname, Action<string> onNicknameChanged)
        {
            inputField.text = inputValue;
            inputField.onEndEdit.AddListener(value=>onNicknameChanged?.Invoke(value));

            if(onChangeNickname is not null)
                onChangeNickname.Subscribe(value=>inputField.text = value)
                    .AddTo(this);
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
        private GameObject _parent;

        public UIIdEditPresenter(UIStringEditView view,
            IUserPreferencesProvider userPreferencesProvider)
        {
            _userPreferencesProvider = userPreferencesProvider;
            _view = view;
        }

        public void Initialize(UIButtonView randomButton, GameObject parent)
        {
            _parent = parent;
            var current = _userPreferencesProvider.Current.User.Id;
            _view.Initialize(current,_userPreferencesProvider.Current.User.OnIdChanged,OnEndEdit);
            randomButton.Initialize(ChangeIdToRandom);
        }

        public void SetVisible(bool isVisible)
        {
            _parent.SetActive(isVisible);
        }

        private void OnEndEdit(string id)
        {
            #if UNITY_EDITOR
            _userPreferencesProvider.Current.User.ChangeId(id);
            #endif
        }

        private void ChangeIdToRandom()
        {
            var id = Guid.NewGuid().ToString();
            _userPreferencesProvider.Current.User.ChangeId(id);
        }
    }
}