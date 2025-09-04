using System.Threading;
using Core.Common;
using Core.UI.Components;
using Core.User;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI.Windows.Views
{
    public class UIWindowProfileSettings : WindowView<UIWindowProfileSettings.ViewModel>
    {
        [SerializeField] private UIStringEditView idEditView;
        [SerializeField] private UIStringEditView stringEditView;
        [SerializeField] private UIProfileSelectorView profileSelectorView;
        [SerializeField] private UIEntitySkinSelectorView entitySkinSelectorView;
        [SerializeField] private UIButtonView closeButton;

        protected override async UniTask BindViewAsync(ViewModel viewModel, CancellationToken ct)
        {
            closeButton.Initialize(viewModel.CloseWindow);
            viewModel.InitializeIdEditView(idEditView);
            viewModel.InitializeNicknameEditView(stringEditView);
            viewModel.InitializeProfileSelectorView(profileSelectorView);
            await viewModel.InitializeSkinViewAsync(entitySkinSelectorView, ct);
        }
        
        public class ViewModel : IViewModel
        {
            private IUserPreferencesProvider _userPreferencesProvider;
            private UIEntitySkinView.Factory _entitySkinViewFactory;
            private SkinMaterialAssetsProvider _skinMaterialAssetsProvider;
            private IWindowsController _windowsController;
            private ProfileSpriteSetsProvider _profileSpriteSetsProvider;
            
            private UIProfileSelectorPresenter _profileSelectorPresenter;
            private UIEntitySkinSelectorPresenter _skinSelectorPresenter;

            public ViewModel(
                IUserPreferencesProvider userPreferencesProvider,
                UIEntitySkinView.Factory entitySkinViewFactory,
                SkinMaterialAssetsProvider skinMaterialAssetsProvider,
                ProfileSpriteSetsProvider profileSpriteSetsProvider,
                IWindowsController windowsController)
            {
                _profileSpriteSetsProvider = profileSpriteSetsProvider;
                _windowsController = windowsController;
                _skinMaterialAssetsProvider = skinMaterialAssetsProvider;
                _entitySkinViewFactory = entitySkinViewFactory;
                _userPreferencesProvider = userPreferencesProvider;
            }

            public async UniTask InitializeSkinViewAsync(UIEntitySkinSelectorView view, CancellationToken ct)
            {
                _skinSelectorPresenter = new UIEntitySkinSelectorPresenter(view, _userPreferencesProvider,
                    _skinMaterialAssetsProvider, _entitySkinViewFactory);
                await _skinSelectorPresenter.InitializeAsync(ct);
            }

            public void InitializeProfileSelectorView(UIProfileSelectorView view)
            {
                _profileSelectorPresenter = new UIProfileSelectorPresenter(view, _userPreferencesProvider, _profileSpriteSetsProvider);
                _profileSelectorPresenter.Initialize();
            }

            public void InitializeNicknameEditView(UIStringEditView view)
            {
                var presenter = new UINicknameEditPresenter(view, _userPreferencesProvider);
                presenter.Initialize();
            }
            public void InitializeIdEditView(UIStringEditView view)
            {
                var presenter = new UIIdEditPresenter(view, _userPreferencesProvider);
                presenter.Initialize();
#if !UNITY_EDITOR
                presenter.SetVisible(false);
#endif
            }

            public void CloseWindow()
            {
                _ = _windowsController.CloseTopAsync();
            }
            
            public void Dispose()
            {
                _profileSelectorPresenter?.Dispose();
                _skinSelectorPresenter?.Dispose();
            }
        }
    }
}