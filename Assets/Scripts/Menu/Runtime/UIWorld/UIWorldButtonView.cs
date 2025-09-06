using System.Threading;
using Core.UI.Components;
using Cysharp.Threading.Tasks;
using Menu.UI;

namespace Menu.UIWorld
{
    public class UIWorldButtonView : UIWorldPointerBounceable
    {
        private ModeButtonViewModel _viewModel;
        
        public void Initialize(ModeButtonViewModel vm)
        {
            _viewModel = vm;
            base.Initialize();
        }

        protected override async UniTask GetOnReleaseAction(CancellationToken token)
        {
            if (_viewModel is not null)
                await _viewModel.ExecuteAsync(token);
        }
    }
}