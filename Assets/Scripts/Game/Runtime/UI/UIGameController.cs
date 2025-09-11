using System.Threading;
using Core.StateMachine;
using Core.UI;
using Core.UI.Components;
using Core.UI.Windows;
using Core.UI.Windows.Views;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Game.UI
{
    public class UIGameController : UIController<UIGame>
    {
        protected IWindowsController WindowsController;
        protected ManualTransitionTrigger<MenuState> _menuTransitionTrigger;

        public UIGameController(
            UIProvider<UIGame> uiProvider,
            ManualTransitionTrigger<MenuState> menuTransitionTrigger,
            IWindowsController windowsController) : base(uiProvider)
        {
            _menuTransitionTrigger = menuTransitionTrigger;
            WindowsController = windowsController;
        }
        public override void Initialize()
        {
            
           InitializeExitButton();
           InitializeSettingsButton();
        }

        protected virtual void InitializeExitButton()
        {
            Provider.UI.ExitButton
                .Initialize(_menuTransitionTrigger.Continue);
        }

        protected virtual void InitializeSettingsButton()
        {
            Provider.UI.SettingsButton
                .Initialize(()=>WindowsController.OpenAsync<UIWindowSettings>().Forget());
        }

        public override void Dispose()
        {
        }
    }
}