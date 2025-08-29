using Core.UI.Components;
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

        protected override void OnBind(ViewModel vm, CompositeDisposable cd)
        {
            vm.Nickname.Subscribe(t =>
            {
                if (nickname !=null)
                {
                    nickname.text = t;
                }
            }).AddTo(cd);
        }

        protected override void OnUnbind()
        {
            if (nickname)
            {
                nickname.text = string.Empty;
            }
        }

        public class ViewModel : IListItemViewModel
        {
            public ReactiveProperty<string> Nickname { get; }

            public ViewModel(string nickname)
            {
                Nickname = new ReactiveProperty<string>(nickname);
            }
        }
    }
}