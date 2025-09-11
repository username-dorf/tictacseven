using System;
using System.Threading;
using Core.AppDebug;
using Core.AppDebug.Components;
using Core.StateMachine;
using Multiplayer.Client;
using UniRx;
using UnityEngine;
using Zenject;

namespace Multiplayer.Server
{
    public class ServerStateProviderDebug : IStateProviderDebug
    {
        public ReactiveProperty<string> State { get; } = new();

        public void ChangeState<T>(T state)
        {
            State.Value = "Server: " + state.GetType().Name;
        }

        public void Dispose()
        {
            State?.Dispose();
        }
    }
    public class ServerStateMachineDebug : IInitializable, IDisposable
    {
        private UIDebugStringView _debugStringViewPrefab;
        private ServerStateProviderDebug _provider;
        private DiContainer _diContainer;
        private UIDebugView _debugView;
        private UIDebugStringView _instance;

        public ServerStateMachineDebug(
            DiContainer diContainer,
            ServerStateProviderDebug provider,
            [InjectOptional(Id = AppDebugConfig.UIDebugViewTag)] UIDebugView debugView,
            [InjectOptional(Id = AppDebugConfig.UIDebugStringViewTag)] UIDebugStringView debugStringViewPrefab)
        {
            _debugView = debugView;
            _diContainer = diContainer;
            _provider = provider;
            _debugStringViewPrefab = debugStringViewPrefab;
        }
        public void Initialize()
        {
            if(_debugView == null || _debugStringViewPrefab == null)
                return;
            
            _instance = _diContainer.InstantiatePrefabForComponent<UIDebugStringView>(_debugStringViewPrefab, _debugView.Container);
            _instance.Initialize(_provider.State);
        }

        public void Dispose()
        {
            if(_instance !=null)
                GameObject.Destroy(_instance.gameObject);
            Debug.Log("ServerStateMachineDebug Dispose");
        }
    }
}