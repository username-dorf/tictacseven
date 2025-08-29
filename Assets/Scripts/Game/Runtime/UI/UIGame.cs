using System;
using Core.UI;
using Core.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [Serializable]
    public class UIGame : IUIView
    {
        [field: SerializeField] public UIRoundResultView[] UIRoundResultViews { get; private set; }
        [field: SerializeField] public UIButtonView ExitButton { get; private set; }
        [field: SerializeField] public UIButtonView SettingsButton { get; private set; }
    }
}