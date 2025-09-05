using System.Threading;
using Core.Common;
using Core.UI.Components;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Core.UI.Windows.Views
{
    public class UIWindowSettings: WindowView<UIWindowSettings.ViewModel>
    {
        [SerializeField] private UIButtonView closeButton;
        [SerializeField] private UIToggleView musicToggle;
        [SerializeField] private UIToggleView soundToggle;

        protected override async UniTask BindViewAsync(ViewModel viewModel, CancellationToken ct)
        {
            closeButton.Initialize(viewModel.CloseWindow);
            musicToggle.Initialize(new UIToggleViewModel(viewModel.Music));
            soundToggle.Initialize(new UIToggleViewModel(viewModel.Sound));
        }
        
        public class ViewModel : IViewModel
        {
            public ReactiveProperty<bool> Sound { get; }
            public ReactiveProperty<bool> Music { get; }
            
            private IWindowsController _windowsController;

            public ViewModel(IWindowsController windowsController,
                IAppPreferencesProvider appPreferences)
            {
                _windowsController = windowsController;

                var preferences = appPreferences.Current;
                Sound = preferences.Sound;
                Music = preferences.Music;
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