using UniState;

namespace Core.StateMachine
{
    public class AppStateMachine : UniState.StateMachine
    {
        protected override void HandleError(StateMachineErrorData errorData)
        {
            // Custom logic here
        }
    }
}