using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;

namespace Core.UI.Windows.Views
{
    public class UIWindowModal : WindowView<UIWindowModal.Payload,UIWindowModal.ViewModel>
    {
        [SerializeField] public UIButtonView acceptButton;
        [SerializeField] public UIButtonView cancelButton;
        [SerializeField] public TMP_Text infoText;
        protected override UniTask BindViewAsync(ViewModel viewModel, CancellationToken ct)
        {
            viewModel.Information
                .Subscribe(OnInformationChanged)
                .AddTo(this);
            viewModel.HasCancel
                .Subscribe(OnHasCancelOption)
                .AddTo(this);
            viewModel.OnAccept
                .Subscribe(acceptButton.Initialize)
                .AddTo(this);
            viewModel.OnCancel
                .Subscribe(cancelButton.Initialize)
                .AddTo(this);
            return UniTask.CompletedTask;
        }

        private void OnInformationChanged(string info)
        {
            infoText.text = info;
        }

        private void OnHasCancelOption(bool hasCancel)
        {
            cancelButton.gameObject.SetActive(hasCancel);
        }

        public class Payload
        {
            public Action OnAccept { get; }
            public Action OnCancel { get; }
            public string Information { get; }

            public Payload(string info, Action onAccept)
            {
                Information = info;
                OnAccept = onAccept;
            }

            public Payload(string info, Action onAccept, Action onCancel)
            {
                Information = info;
                OnAccept = onAccept;
                OnCancel = onCancel;
            }
        }
        public class ViewModel : IViewModel, IPayloadReceiver<Payload>
        {
            private IWindowsController _windowsController;
            public ReactiveProperty<string> Information { get; private set; }
            public ReactiveProperty<bool> HasCancel { get; private set; }
            public ReactiveProperty<Action> OnAccept { get; private set; }
            public ReactiveProperty<Action> OnCancel { get; private set; }

            public ViewModel(IWindowsController windowsController)
            {
                _windowsController = windowsController;
            }

            public void SetPayload(Payload payload)
            {
                void OnAcceptAnClose()
                {
                    payload?.OnAccept?.Invoke();
                    Close();
                }

                void OnCancelAndClose()
                {
                    payload?.OnCancel?.Invoke();
                    Close();
                }
                
                Information = new ReactiveProperty<string>(payload.Information);
                OnAccept = new ReactiveProperty<Action>(OnAcceptAnClose);
                var hasCancel = payload.OnCancel != null;
                HasCancel = new ReactiveProperty<bool>(hasCancel);
                OnCancel = hasCancel ? new ReactiveProperty<Action>(OnCancelAndClose) : new ReactiveProperty<Action>();
            }

            private void Close()
            {
                _ = _windowsController.CloseTopAsync();
            }

            public void Dispose()
            {
                Information?.Dispose();
                HasCancel?.Dispose();
                OnAccept?.Dispose();
                OnCancel?.Dispose();
            }
        }
    }
}