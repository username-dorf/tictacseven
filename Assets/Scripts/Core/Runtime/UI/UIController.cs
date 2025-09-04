using System;
using Core.UI.Components;
using Zenject;

namespace Core.UI
{
    public interface IUIView
    {
        
    }

    public interface IUIController<T> : IInitializable, IDisposable where T : IUIView
    {
        
    }
    public abstract class UIController<T> : IUIController<T> where T : IUIView
    {
        protected UIProvider<T> Provider { get; private set; }
        public UIController(UIProvider<T> provider)
        {
            Provider = provider;
        }

        public abstract void Initialize();

        public abstract void Dispose();
    }
}