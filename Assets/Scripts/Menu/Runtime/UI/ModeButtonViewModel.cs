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
        private SignalBus _signalBus;

        public ModeButtonViewModel(Func<CancellationToken, UniTask> actionFactory, SignalBus signalBus)
        {
            _signalBus = signalBus;
            _actionFactory = actionFactory;
        }

        public async UniTask ExecuteAsync(CancellationToken ct = default)
        {
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

        public class Factory : PlaceholderFactory<Func<CancellationToken, UniTask>,ModeButtonViewModel>
        {
            
        }
    }
}