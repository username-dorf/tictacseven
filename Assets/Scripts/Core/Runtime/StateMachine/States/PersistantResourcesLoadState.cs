using System.Threading;
using Core.Common;
using Core.UI.Windows;
using Core.User;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Core.StateMachine
{
    public class PersistantResourcesLoadState : IState
    {
        private IStateMachine _stateMachine;
        private ProfileSpriteSetsProvider _profileSpritesProvider;
        private WindowsAssetsProvider _windowsAssetsProvider;
        private SkinMaterialAssetsProvider _skinMaterialAssetsProvider;

        public PersistantResourcesLoadState(
            IStateMachine stateMachine,
            WindowsAssetsProvider windowsAssetsProvider,
            ProfileSpriteSetsProvider profileSpritesProvider,
            SkinMaterialAssetsProvider skinMaterialAssetsProvider )
        {
            _skinMaterialAssetsProvider = skinMaterialAssetsProvider;
            _windowsAssetsProvider = windowsAssetsProvider;
            _stateMachine = stateMachine;
            _profileSpritesProvider = profileSpritesProvider;
        }
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await _profileSpritesProvider.LoadAssetsByLabels(ct, Addressables.MergeMode.Intersection, "profile",
                "sprite");

            await _windowsAssetsProvider.LoadAssetsByLabels(default, Addressables.MergeMode.Union, "window");

            await _skinMaterialAssetsProvider.LoadAll(ct, "base");
            
            await _stateMachine.ChangeStateAsync<MenuState>(ct);
        }

        public UniTask ExitAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _profileSpritesProvider?.Dispose();
            _windowsAssetsProvider?.Dispose();
            _skinMaterialAssetsProvider?.Dispose();
        }
    }
}