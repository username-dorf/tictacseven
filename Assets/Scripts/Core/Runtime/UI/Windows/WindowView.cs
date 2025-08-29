using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Core.UI.Windows
{
    public abstract class WindowView<TViewModel> : WindowViewBase
        where TViewModel : class, IViewModel
    {
        protected TViewModel VM { get; private set; }

        [Inject]
        public void Construct(TViewModel viewModel)
        {
            VM = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        protected sealed override UniTask BindAsync(CancellationToken ct)
            => BindViewAsync(VM, ct);

        protected sealed override UniTask UnbindAsync(CancellationToken ct)
        {
            VM?.Dispose();
            return base.UnbindAsync(ct);
        }

        protected abstract UniTask BindViewAsync(TViewModel viewModel, CancellationToken ct);
    }

    public abstract class WindowView<TPayload, TViewModel> : WindowViewBase, IPayloadedWindow<TPayload>
        where TViewModel : class, IViewModel, IPayloadReceiver<TPayload>
    {
        protected TViewModel VM { get; private set; }

        [Inject]
        public void Construct(TViewModel viewModel)
        {
            VM = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        public void SetPayload(TPayload payload)
        {
            VM.SetPayload(payload);
        }

        protected sealed override UniTask BindAsync(CancellationToken ct)
            => BindViewAsync(VM, ct);

        protected sealed override UniTask UnbindAsync(CancellationToken ct)
        {
            VM?.Dispose();
            return base.UnbindAsync(ct);
        }

        protected abstract UniTask BindViewAsync(TViewModel viewModel, CancellationToken ct);
    }
}