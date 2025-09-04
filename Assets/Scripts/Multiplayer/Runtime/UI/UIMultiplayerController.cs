using Core.StateMachine;
using Core.UI.Components;
using Core.UI.Windows;
using FishNet;
using Game.UI;
using Multiplayer.Contracts;

namespace Multiplayer.UI
{
    public class UIMultiplayerController : UIGameController
    {
        public UIMultiplayerController(
            UIProvider<UIGame> uiProvider,
            IStateMachine stateMachine,
            IWindowsController windowsController)
            : base(uiProvider, stateMachine, windowsController)
        {
        }

        protected override void InitializeExitButton()
        {
            Provider.UI.ExitButton
                .Initialize(()=>InstanceFinder.ClientManager.Broadcast(new ClientLeaveSessionNotice()));
        }
    }
}