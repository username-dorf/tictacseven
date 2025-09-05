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
        protected IStateMachine StateMachine;
        protected IWindowsController WindowsController;

        public UIGameController(
            UIProvider<UIGame> uiProvider,
            IStateMachine stateMachine,
            IWindowsController windowsController) : base(uiProvider)
        {
            WindowsController = windowsController;
            StateMachine = stateMachine;
        }
        public override void Initialize()
        {
            
           InitializeExitButton();
           InitializeSettingsButton();
        }

        protected virtual void InitializeExitButton()
        {
            Provider.UI.ExitButton
                .Initialize(()=>StateMachine.ChangeStateAsync<MenuState>(CancellationToken.None));
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