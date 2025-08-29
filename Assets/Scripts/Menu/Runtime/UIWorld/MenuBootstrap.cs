using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Menu.Runtime.UIWorld
{
    public class MenuBootstrap : IInitializable, IDisposable
    {
        private MenuFieldViewFactory _fieldViewFactory;
        private CancellationTokenSource _cancellationTokenSource;
        private MenuFieldSubviewFactory _fieldSubviewFactory;

        public MenuBootstrap(MenuFieldViewFactory fieldViewFactory,
            MenuFieldSubviewFactory fieldSubviewFactory)
        {
            _fieldSubviewFactory = fieldSubviewFactory;
            _fieldViewFactory = fieldViewFactory;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        public void Initialize()
        {
            CreateAsync(_cancellationTokenSource.Token).Forget();
        }

        private async UniTask CreateAsync(CancellationToken ct)
        {
            try
            {
                var fieldView = await _fieldViewFactory.CreateAsync(ct);
                await fieldView.PlayScaleAsync(ct);
                var fieldSubview = await _fieldSubviewFactory.CreateAsync(ct);
                await fieldSubview.PlayScaleAsync(ct);
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        public void Dispose()
        {
            if(!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}