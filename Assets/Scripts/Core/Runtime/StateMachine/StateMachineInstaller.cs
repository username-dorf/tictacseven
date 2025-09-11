using UniState;
using Zenject;

namespace Core.StateMachine
{
    public class StateMachineInstaller : Installer<StateMachineInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindStateMachine<IStateMachine, AppStateMachine>();
            StatesRegistration.Install(Container);
        }
    }
}
