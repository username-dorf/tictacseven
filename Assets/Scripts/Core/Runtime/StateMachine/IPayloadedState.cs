namespace Core.StateMachine
{
    public interface IPayloadedState<in TPayload> : IState
    {
        void SetPayload(TPayload payload);
    }
}