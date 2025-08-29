using System.Threading;
using Core.UI.Components;
using Core.UI.Windows;
using Cysharp.Threading.Tasks;
using Multiplayer.Lan;
using Multiplayer.UI.Components;
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
        private ViewModel _viewModel;

        protected override UniTask BindViewAsync(ViewModel viewModel, CancellationToken ct)
        {
            _viewModel = viewModel;
            
            closeButton.Initialize(_viewModel.CloseWindow);

            cancelButton.OnClickAsObservable()
                .Subscribe(_=>_viewModel.CloseWindow())
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
        
        public class ViewModel : IViewModel
        {
            private LanController _lanController;
            private IWindowsController _windowsController;
            public ListViewModel<UIViewHostView.ViewModel> ViewModels { get; }

            public ViewModel(LanController lanController, IWindowsController windowsController)
            {
                _windowsController = windowsController;
                _lanController = lanController;
                ViewModels = new ListViewModel<UIViewHostView.ViewModel>();
            }

            public void StartDiscovery()
            {
                _lanController.StartDiscovery(OnHostDiscovered);
            }

            public void StopDiscovery()
            {
                _lanController.StopDiscovery();
                _ = _windowsController.CloseTopAsync();

            }
            public void CloseWindow()
            {
                StopDiscovery();
            }

            private void OnHostDiscovered(string nickname, string device)
            {
                ViewModels.Add(new UIViewHostView.ViewModel($"User #{nickname}"));
            }

            public void Dispose()
            {
                _lanController.StopDiscovery();
            }
        }
    }
}