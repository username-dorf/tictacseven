using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI.Windows.Views
{
    public class UIWindowSettings: WindowView<UIWindowSettings.ViewModel>
    {
        [SerializeField] private UIButtonView closeButton;

        protected override async UniTask BindViewAsync(ViewModel viewModel, CancellationToken ct)
        {
            closeButton.Initialize(viewModel.CloseWindow);
        }
        
        public class ViewModel : IViewModel
        {
            
            private IWindowsController _windowsController;

            public ViewModel(IWindowsController windowsController)
            {
                _windowsController = windowsController;
            }

            public void CloseWindow()
            {
                _ = _windowsController.CloseTopAsync();
            }
            
            public void Dispose()
            {
                
            }
        }
    }
}