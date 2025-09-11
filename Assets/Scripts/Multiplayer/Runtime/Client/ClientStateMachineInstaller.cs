using System.Threading;
using Multiplayer.Client.States;
using UniState;
using Zenject;

namespace Multiplayer.Client
{
    public class ClientStateMachineInstaller
    {
        public void Install(DiContainer sub)
        {
            sub.BindStateMachine<IStateMachine, ClientStateMachine>();
            
            sub.BindState<InitialSubstate>();
            sub.BindState<TurnSubstate>();
            sub.BindState<ServerSyncSubstate>();
            sub.BindState<RoundResultSubstate>();
            sub.BindState<RoundClearSubstate>();
            sub.BindState<WaitTurnSubstate>();

            sub.BindInterfacesTo<Game.States.GameSubstatesFacade>()
                .AsSingle();

            sub.BindInterfacesAndSelfTo<ClientStateMachineDisposable>()
                .AsSingle();
        }
    }
}