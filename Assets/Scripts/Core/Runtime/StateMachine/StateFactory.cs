using Zenject;

namespace Core.StateMachine
{
    public class StateFactory : IFactory<string,IState>
    {
        DiContainer _container;

        public StateFactory(DiContainer container)
        {
            _container = container;
        }
        
        public IState Create(string id)
        {
            return _container.ResolveId<IState>(id);
        }
    }
}