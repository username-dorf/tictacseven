using System;
using Core.UI;
using Menu.UIWorld;
using UnityEngine;

namespace Menu.Runtime.UI
{
    [Serializable]
    public class UIMenu : IUIView
    {
        public UIWorldButtonView ClassicButtonView { get; private set; }
        public UIWorldButtonView ArcadeButtonView { get; private set; }
        public UIWorldButtonView SettingsButtonView { get; private set; }
        public UIWorldButtonView SocialButtonView { get; private set; }
        public UIWorldButtonView ProfileSettingsButtonView { get; private set; }
        
        public MultiplayerButtonsGroup MultiplayerGroup { get; private set; }

        public void BindClassicButtonView(UIWorldButtonView view)
        {
            ClassicButtonView = view;
        }

        public void BindSettingsButton(UIWorldButtonView view)
        {
            SettingsButtonView = view;
        } 
        
        public void BindProfileSettingsButton(UIWorldButtonView view)
        {
            ProfileSettingsButtonView = view;
        }
        public void BindSocialButton(UIWorldButtonView view)
        {
            SocialButtonView = view;
        }
        public void BindArcadeButton(UIWorldButtonView view)
        {
            ArcadeButtonView = view;
        }

        public void BindMultiplayerButtons(UIWorldButtonView createHost, UIWorldButtonView connectClient)
        {
            MultiplayerGroup = new MultiplayerButtonsGroup(createHost, connectClient);
        }

        public class MultiplayerButtonsGroup
        {
            public UIWorldButtonView CreateHostButton { get; }
            public UIWorldButtonView ConnectClientButton { get; }

            public MultiplayerButtonsGroup(UIWorldButtonView createHostButton, UIWorldButtonView connectClientButton)
            {
                CreateHostButton = createHostButton;
                ConnectClientButton = connectClientButton;
            }
        }
    }
}