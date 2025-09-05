using Core.StateMachine;
using Core.UI.Components;
using Core.UI.Windows;
using Core.User;
using FishNet;
using Game.UI;
using Multiplayer.Contracts;

namespace Multiplayer.UI
{
    public class UIMultiplayerController : UIGameController
    {
        private IUserPreferencesProvider _userPreferencesProvider;

        public UIMultiplayerController(
            UIProvider<UIGame> uiProvider,
            IStateMachine stateMachine,
            IUserPreferencesProvider userPreferencesProvider,
            IWindowsController windowsController)
            : base(uiProvider, stateMachine, windowsController)
        {
            _userPreferencesProvider = userPreferencesProvider;
        }

        protected override void InitializeExitButton()
        {
            var clientId = _userPreferencesProvider.Current.User.Id;
            Provider.UI.ExitButton
                .Initialize(()=>InstanceFinder.ClientManager.Broadcast(new ClientLeaveSessionNotice(){ClientId = clientId}));
        }
    }
}