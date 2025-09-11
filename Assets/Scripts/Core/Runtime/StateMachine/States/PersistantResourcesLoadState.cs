using System.Threading;
using Core.Audio;
using Core.Common;
using Core.UI.Windows;
using Core.User;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UniState;

namespace Core.StateMachine
{
    public class PersistantResourcesLoadState : StateBase
    {
        private ProfileSpriteSetsProvider _profileSpritesProvider;
        private WindowsAssetsProvider _windowsAssetsProvider;
        private SkinMaterialAssetsProvider _skinMaterialAssetsProvider;
        private AudioAssetsPreloader _audioAssetsPreloader;

        public PersistantResourcesLoadState(
            WindowsAssetsProvider windowsAssetsProvider,
            ProfileSpriteSetsProvider profileSpritesProvider,
            SkinMaterialAssetsProvider skinMaterialAssetsProvider,
            AudioAssetsPreloader audioAssetsPreloader)
        {
            _audioAssetsPreloader = audioAssetsPreloader;
            _skinMaterialAssetsProvider = skinMaterialAssetsProvider;
            _windowsAssetsProvider = windowsAssetsProvider;
            _profileSpritesProvider = profileSpritesProvider;
        }
        public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
        {
            await _audioAssetsPreloader.LoadAssetsAsync(token);
            await _profileSpritesProvider.LoadAssetsByLabels(token, Addressables.MergeMode.Intersection, "profile",
                "sprite");

            await _windowsAssetsProvider.LoadAssetsByLabels(default, Addressables.MergeMode.Union, "window");

            await _skinMaterialAssetsProvider.LoadAll(token, "base");
            
            return Transition.GoTo<MenuState>();
        }
    }
}