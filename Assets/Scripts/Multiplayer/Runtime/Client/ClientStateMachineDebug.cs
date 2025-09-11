using System;
using System.Threading;
using Core.AppDebug;
using Core.AppDebug.Components;
using Core.StateMachine;
using UniRx;
using UnityEngine;
using Zenject;

namespace Multiplayer.Client
{
    public class ClientStateProviderDebug : IStateProviderDebug
    {
        public ReactiveProperty<string> State { get; } = new();

        public void ChangeState<T>(T state)
        {
            State.Value = "Client: " + state.GetType().Name;
        }

        public void Dispose()
        {
            State?.Dispose();
        }
    }
    public class ClientStateMachineDebug : IInitializable, IDisposable
    {
        private CancellationTokenSource _cts;
        private UIDebugStringView _debugStringViewPrefab;
        private IStateProviderDebug _provider;
        private DiContainer _diContainer;
        private UIDebugView _debugView;
        private UIDebugStringView _instance;

        public ClientStateMachineDebug(
            DiContainer diContainer,
            IStateProviderDebug provider,
            [InjectOptional(Id = AppDebugConfig.UIDebugViewTag)] UIDebugView debugView,
            [InjectOptional(Id = AppDebugConfig.UIDebugStringViewTag)] UIDebugStringView debugStringViewPrefab)
        {
            _debugView = debugView;
            _diContainer = diContainer;
            _provider = provider;
            _debugStringViewPrefab = debugStringViewPrefab;
            _cts = new CancellationTokenSource();
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
            
            _cts?.Dispose();
        }
    }
}