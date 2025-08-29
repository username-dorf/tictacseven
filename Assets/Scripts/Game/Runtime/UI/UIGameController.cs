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
        private IStateMachine _stateMachine;
        private IWindowsController _windowsController;

        public UIGameController(
            UIProvider<UIGame> uiProvider,
            IStateMachine stateMachine,
            IWindowsController windowsController) : base(uiProvider)
        {
            _windowsController = windowsController;
            _stateMachine = stateMachine;
        }
        public override void Initialize()
        {
            Provider.UI.ExitButton
                .Initialize(()=>_stateMachine.ChangeStateAsync<MenuState>(CancellationToken.None));
            
            Provider.UI.SettingsButton
                .Initialize(()=>_windowsController.OpenAsync<UIWindowProfileSettings>().Forget());
        }

        public override void Dispose()
        {
        }
    }
}