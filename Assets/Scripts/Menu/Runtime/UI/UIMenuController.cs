using Core.StateMachine;
using Core.UI;
using Core.UI.Components;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Cysharp.Threading.Tasks;
using Menu.Runtime.UIWorld;
using Menu.UI;
using Multiplayer.UI.Windows.Views;

namespace Menu.Runtime.UI
{
    public class UIMenuController : UIController<UIMenu>
    {
        private IWindowsController _windowsController;
        private ModeButtonViewModel.Factory _viewModelFactory;
        private ManualTransitionTrigger<GameState> _gameTransitionTrigger;

        public UIMenuController(
            UIProvider<UIMenu> uiProvider,
            IWindowsController windowsController,
            ModeButtonViewModel.Factory viewModelFactory,
            ManualTransitionTrigger<GameState> gameTransitionTrigger) : base(uiProvider)
        {
            _gameTransitionTrigger = gameTransitionTrigger;
            _viewModelFactory = viewModelFactory;
            _windowsController = windowsController;
        }
        public override void Initialize()
        {
            
        }

        public void BindButtonViews(MenuFieldView menuFieldView)
        {
            Provider.UI.BindClassicButtonView(menuFieldView.ClassicButtonView);
            var classicButtonViewModel =
                _viewModelFactory.Create(_gameTransitionTrigger.Continue);
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