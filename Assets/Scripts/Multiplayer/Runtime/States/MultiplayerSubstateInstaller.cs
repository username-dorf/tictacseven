using System;

namespace Multiplayer.States
{
    public interface IMultiplayerSubstateResolver: IDisposable
    {
        public T Resolve<T>();
    }
    public class MultiplayerSubstateInstaller
    {
        
    }
}