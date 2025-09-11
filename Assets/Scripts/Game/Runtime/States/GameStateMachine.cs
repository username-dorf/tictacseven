using UniState;
using UnityEngine;

namespace Game.States
{
    public sealed class GameStateMachine : StateMachine
    {
        protected override void HandleError(StateMachineErrorData errorData)
        {
            Debug.LogException(errorData.Exception);
        }
    }
}