using System;
using System.Threading;
using Core.UI.Components;
using Core.UI.Windows;
using Core.User;
using Cysharp.Threading.Tasks;
using Multiplayer.Connection;
using Multiplayer.UI.Components;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Windows.Views
{
    public class UIWindowConnectClient: WindowView<UIWindowConnectClient.ViewModel>
    {
        [SerializeField] private UIButtonView closeButton;
        [SerializeField] private Button cancelButton;

        [SerializeField] private UIVirtualizedScrollView listView;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private UIViewHostView itemPrefab;

        [SerializeField] private Transform connectingOverlay;
        [SerializeField] private TMP_Text connectingMessage;
        
        private ViewModel _viewModel;

        protected override UniTask BindViewAsync(ViewModel viewModel, CancellationToken ct)
        {
            _viewModel = viewModel;
            
            closeButton.Initialize(_viewModel.CloseWindow);

            cancelButton.OnClickAsObservable()
                .Subscribe(_=>_viewModel.CloseWindow())
                .AddTo(this);
            _viewModel.ConnectingText
                .Subscribe(OnConnectingMessage)
                .AddTo(this);
            _viewModel.IsConnecting
                .Subscribe(OnConnecting)
                .AddTo(this);
            
            
            var prefabLayoutElement = itemPrefab.GetComponent<LayoutElement>();
            var heightProvider = new ConstHeightProvider(prefabLayoutElement.preferredHeight);

            listView.Initialize(viewModel.ViewModels, heightProvider);
            return UniTask.CompletedTask;
        }
        protected override UniTask OnAfterOpenAsync(CancellationToken ct)
        {
            _viewModel.StartDiscovery();
            return UniTask.CompletedTask;
        }

        private void OnConnecting(bool isConnecting)
        {
            connectingOverlay.gameObject.SetActive(isConnecting);
        }

        private void OnConnectingMessage(string message)
        {
            connectingMessage.text = message;
        }
        
        public class ViewModel : IViewModel
        {
            private SessionController _sessionController;
            private IWindowsController _windowsController;
            private CompositeDisposable _disposable;
            private JoinResponseListener _joinResponseListener;
            public ListViewModel<UIViewHostView.ViewModel> ViewModels { get; }
            public ReactiveProperty<bool> IsConnecting { get; }
            public ReactiveProperty<string> ConnectingText { get; }
            private IDisposable _respSub;


            public ViewModel(SessionController sessionController, IWindowsController windowsController, JoinResponseListener joinResponseListener)
            {
                _joinResponseListener = joinResponseListener;
                _windowsController = windowsController;
                _sessionController = sessionController;
                ViewModels = new ListViewModel<UIViewHostView.ViewModel>();
                IsConnecting = new ReactiveProperty<bool>(false);
                ConnectingText = new ReactiveProperty<string>(string.Empty);
                _disposable = new CompositeDisposable();
            }

            public void StartDiscovery()
            {
                _sessionController.StartDiscovery(OnHostDiscovered);
            }

            public void StopDiscovery()
            {
                _sessionController.StopDiscovery();
                _ = _windowsController.CloseTopAsync();

            }
            public void CloseWindow()
            {
                StopDiscovery();
            }

            private void OnHostDiscovered(UserPreferencesDto preferencesModel, string ip)
            {
                var viewModel = new UIViewHostView.ViewModel(preferencesModel, ip);
                
                viewModel.OnConnectRequested
                    .Subscribe(data=>OnConnectRequested(data.nickname,data.ip))
                    .AddTo(_disposable);
                
                ViewModels.Add(viewModel);
            }

            private void OnConnectRequested(string nickname, string ip)
            {

                IsConnecting.Value = true;
                ConnectingText.Value = $"Connecting to {nickname}…";
                
                _respSub?.Dispose();
                _respSub = OnJoinResponseBehaviour();
                
                _joinResponseListener.Start();
                _sessionController.ConnectTo(ip);
            }

            private IDisposable OnJoinResponseBehaviour()
            {
                return _joinResponseListener.OnResponse
                    .Take(1)
                    .ObserveOnMainThread()
                    .Subscribe(
                        resp =>
                        {
                            IsConnecting.Value = false;
                            if (resp.Accepted)
                            {
                                //connected
                            }
                            else
                            {
                                Debug.LogError($"Join rejected: {resp.Reason}");
                            }
                            _joinResponseListener.Stop();
                        },
                        ex =>
                        {
                            IsConnecting.Value = false;
                            if (ex is TimeoutException)
                            {
                                IsConnecting.Value = false;
                                Debug.LogError("Join timeout: host did not respond.");
                            }
                            else
                                Debug.LogException(ex);
                            _joinResponseListener.Stop();
                        });

            }
            public void Dispose()
            {
                _sessionController.StopDiscovery();
                _disposable?.Dispose();
            }
        }
    }
}