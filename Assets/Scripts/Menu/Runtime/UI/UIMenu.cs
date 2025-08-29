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
        public UIWorldButtonView SettingsButtonView { get; private set; }
        public UIWorldButtonView ProfileSettingsButtonView { get; private set; }

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
        
    }
}