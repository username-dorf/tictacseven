using UniState;

namespace Game.States
{
    public abstract class GameSubstate : StateBase
    {
       
    }
    
    public abstract class GameSubstate<TPayload> : StateBase<TPayload>
    {
       
    }
}