using UniState;

namespace Multiplayer.Server.States
{
    public abstract class ServerSubstate : StateBase
    {
       
    }
    
    public abstract class ServerSubstate<TPayload> : StateBase<TPayload>
    {
        
    }
}