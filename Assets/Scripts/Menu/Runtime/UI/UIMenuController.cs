using System;
using System.Threading;
using Core.StateMachine;
using Core.UI;
using Core.UI.Components;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Cysharp.Threading.Tasks;
using Menu.Runtime.UIWorld;
using Menu.UI;
using Menu.UIWorld;
using Multiplayer.UI.Windows.Views;
using Zenject;

namespace Menu.Runtime.UI
{
    public class UIMenuController : UIController<UIMenu>
    {
        private IStateMachine _stateMachine;
        private IWindowsController _windowsController;
        private ModeButtonViewModel.Factory _viewModelFactory;

        public UIMenuController(
            UIProvider<UIMenu> uiProvider,
            IStateMachine stateMachine,
            IWindowsController windowsController,
            ModeButtonViewModel.Factory viewModelFactory) : base(uiProvider)
        {
            _viewModelFactory = viewModelFactory;
            _windowsController = windowsController;
            _stateMachine = stateMachine;
        }
        public override void Initialize()
        {
            
        }

        public void BindButtonViews(MenuFieldView menuFieldView)
        {
            Provider.UI.BindClassicButtonView(menuFieldView.ClassicButtonView);
            var classicButtonViewModel =
                _viewModelFactory.Create(ct => _stateMachine.ChangeStateAsync<GameState>(CancellationToken.None));
            Provider.UI.ClassicButtonView.Initialize(classicButtonViewModel);

            Provider.UI.BindMultiplayerButtons(menuFieldView.CreateHostButtonView,
                menuFieldView.ConnectClientButtonView);
            var createHostButtonViewModel =
                _viewModelFactory.Create(ct => _windowsController.OpenAsync<UIWindowCreateHost>(ct));
            Provider.UI.MultiplayerGroup.CreateHostButton.Initialize(createHostButtonViewModel);
            
            var connectClientButtonViewModel =
                _viewModelFactory.Create(ct => _windowsController.OpenAsync<UIWindowConnectClient>(ct));
            Provider.UI.MultiplayerGroup.ConnectClientButton.Initialize(connectClientButtonViewModel);
            
            Provider.UI.BindArcadeButton(menuFieldView.ArcadeButtonView);
            var arcadeButtonViewModel =
                _viewModelFactory.Create(ct => { return UniTask.CompletedTask; });
            Provider.UI.ArcadeButtonView.Initialize(arcadeButtonViewModel);
        }

        public void BindSubviewButtonViews(MenuFieldSubview subview)
        {
            Provider.UI.BindSettingsButton(subview.SettingsButton);
            var settingsButtonViewModel =
                _viewModelFactory.Create(ct => _windowsController.OpenAsync<UIWindowSettings>(ct));
            Provider.UI.SettingsButtonView.Initialize(settingsButtonViewModel);
            
            Provider.UI.BindProfileSettingsButton(subview.ProfileButton);
            var profileSettingsButtonViewModel =
                _viewModelFactory.Create(ct => _windowsController.OpenAsync<UIWindowProfileSettings>(ct));
            Provider.UI.ProfileSettingsButtonView.Initialize(profileSettingsButtonViewModel);
            
            Provider.UI.BindSocialButton(subview.SocialButton);
            var socialButtonViewModel =
                _viewModelFactory.Create(ct => { return UniTask.CompletedTask; });
            Provider.UI.SocialButtonView.Initialize(socialButtonViewModel);
        }

        public override void Dispose()
        {
        }
    }
}