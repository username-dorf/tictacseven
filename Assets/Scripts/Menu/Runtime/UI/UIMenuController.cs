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

        public UIMenuController(UIProvider<UIMenu> uiProvider, IStateMachine stateMachine, IWindowsController windowsController) : base(uiProvider)
        {
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
                new ModeButtonViewModel(ct => _stateMachine.ChangeStateAsync<GameState>(CancellationToken.None));
            Provider.UI.ClassicButtonView.Initialize(classicButtonViewModel);

            Provider.UI.BindMultiplayerButtons(menuFieldView.CreateHostButtonView,
                menuFieldView.ConnectClientButtonView);
            var createHostButtonViewModel =
                new ModeButtonViewModel(ct => _windowsController.OpenAsync<UIWindowCreateHost>(ct));
            Provider.UI.MultiplayerGroup.CreateHostButton.Initialize(createHostButtonViewModel);
            
            var connectClientButtonViewModel =
                new ModeButtonViewModel(ct => _windowsController.OpenAsync<UIWindowConnectClient>(ct));
            Provider.UI.MultiplayerGroup.ConnectClientButton.Initialize(connectClientButtonViewModel);
        }

        public void BindSubviewButtonViews(MenuFieldSubview subview)
        {
            Provider.UI.BindSettingsButton(subview.SettingsButton);
            var settingsButtonViewModel =
                new ModeButtonViewModel(ct => _windowsController.OpenAsync<UIWindowSettings>(ct));
            Provider.UI.SettingsButtonView.Initialize(settingsButtonViewModel);
            
            Provider.UI.BindProfileSettingsButton(subview.ProfileButton);
            var profileSettingsButtonViewModel =
                new ModeButtonViewModel(ct => _windowsController.OpenAsync<UIWindowProfileSettings>(ct));
            Provider.UI.ProfileSettingsButtonView.Initialize(profileSettingsButtonViewModel);
        }

        public override void Dispose()
        {
        }
    }
}