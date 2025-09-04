using Zenject;

namespace Core.StateMachine
{
    public sealed class StateFactory
    {
        private readonly DiContainer _container;
        public StateFactory(DiContainer container) => _container = container;

        public TState Get<TState>() where TState : IState
            => (TState)_container.ResolveId<IState>(typeof(TState).Name);
    }
}