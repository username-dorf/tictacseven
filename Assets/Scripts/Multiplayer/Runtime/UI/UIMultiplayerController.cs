using Core.StateMachine;
using Core.UI.Components;
using Core.UI.Windows;
using Core.User;
using Cysharp.Threading.Tasks;
using FishNet;
using Game.UI;
using Multiplayer.Client;
using Multiplayer.Contracts;
using Zenject;

namespace Multiplayer.UI
{
    public class UIMultiplayerController : UIGameController
    {
        private LazyInject<ISessionExitClient> _sessionExitClient;

        public UIMultiplayerController(
            UIProvider<UIGame> uiProvider,
            ManualTransitionTrigger<MenuState> menuTransitionTrigger,
            IWindowsController windowsController,
            LazyInject<ISessionExitClient> sessionExitClient)
            : base(uiProvider, menuTransitionTrigger, windowsController)
        {
            _sessionExitClient = sessionExitClient;
        }

        protected override void InitializeExitButton()
        {
            Provider.UI.ExitButton
                .Initialize(()=>_sessionExitClient.Value.LeaveByUserAsync().Forget());
        }
        
    }
}