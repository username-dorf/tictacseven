using UniState;
using UnityEngine;

namespace Multiplayer.Server.States
{
    public class ServerStateMachine: StateMachine
    {
        protected override void HandleError(StateMachineErrorData errorData)
        {
            Debug.LogException(errorData.Exception);
        }
    }
}