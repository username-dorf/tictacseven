using System;
using System.Threading;
using Core.Audio;
using Core.Audio.Signals;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Menu.UI
{
    public class ModeButtonViewModel : IDisposable
    {
        private readonly Func<CancellationToken, UniTask> _actionFactory;
        private readonly Action _action;
        private SignalBus _signalBus;

        public ModeButtonViewModel(Func<CancellationToken, UniTask> actionFactory, SignalBus signalBus)
        {
            _signalBus = signalBus;
            _actionFactory = actionFactory;
        }

        public ModeButtonViewModel(Action action, SignalBus signalBus)
        {
            _action = action;
            _signalBus = signalBus;
        }
        public void Execute()
        {
            ExecuteSfx();
            _action?.Invoke();
        }

        public async UniTask ExecuteAsync(CancellationToken ct = default)
        {
            if (_actionFactory is null)
            {
                Execute();
                return;
            }
            
            try
            {
                ExecuteSfx();
                await _actionFactory(ct);
            }
            catch (OperationCanceledException)
            {
            
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }

        public void ExecuteSfx()
        {
            var randomPitch = Random.Range(0.9f, 1);
            _signalBus?.Fire(new SignalSfxPlay(SfxKey.Ui_Pop, pitch:randomPitch));
        }

        public void Dispose()
        {
        
        }

        public class Factory
        {
            private SignalBus _signalBus;

            public Factory(SignalBus signalBus)
            {
                _signalBus = signalBus;
            }
            public ModeButtonViewModel Create(Func<CancellationToken, UniTask> actionFactory)
            {
                return new ModeButtonViewModel(actionFactory, _signalBus);
            }

            public ModeButtonViewModel Create(Action action)
            {
                return new ModeButtonViewModel(action, _signalBus);
            }
            
        }
        
    }
}