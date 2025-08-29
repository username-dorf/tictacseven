using System;
using Core.AssetProvider;

namespace Core.UI.Windows
{
    public sealed class WindowsAssetsProvider : AssetsProvider<WindowViewBase, Type>
    {
        public WindowsAssetsProvider()
            : base(keySelector: w => w.GetType())
        { }
    }
}