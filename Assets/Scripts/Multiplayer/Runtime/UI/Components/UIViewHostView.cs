using System;
using Core.UI.Components;
using Core.User;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Multiplayer.UI.Components
{
    public class UIViewHostView : UIBaseListItemView<UIViewHostView.ViewModel>
    {
        [SerializeField] private TMP_Text nickname;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private UIButtonView connectButton;

        protected override void OnBind(ViewModel viewModel, CompositeDisposable disposable)
        {
            viewModel.Nickname
                .Subscribe(OnChangeNickname)
                .AddTo(disposable);
            
            connectButton.Initialize(viewModel.Connect);
        }

        protected override void OnUnbind()
        {
            OnChangeNickname(string.Empty);
        }

        protected void OnChangeNickname(string nickname)
        {
            if(this.nickname != null)
                this.nickname.text = nickname;
        }

        public new class ViewModel : IListItemViewModel
        {
            public ReactiveProperty<string> Nickname { get; }
            public ReactiveProperty<string> Ip { get; }
            public IObservable<(string nickname, string ip)> OnConnectRequested => _onConnectRequested;

            private Subject<(string nickname, string ip)> _onConnectRequested;

            public ViewModel(UserPreferencesDto preferencesModel, string ip)
            {
                _onConnectRequested = new Subject<(string nickname, string ip)>();
                Nickname = new ReactiveProperty<string>(preferencesModel.nickname);
                Ip = new ReactiveProperty<string>(ip);
            }

            public void Connect()
            {
                _onConnectRequested?.OnNext((Nickname.Value, Ip.Value));
            }
        }
    }
}