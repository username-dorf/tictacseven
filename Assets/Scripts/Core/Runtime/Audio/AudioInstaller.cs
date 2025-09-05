using Core.Audio.Players;
using Core.Audio.Signals;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Core.Audio
{
    public class AudioInstaller: Installer<AudioInstaller>
    {
        private string[] _sfxLabels = { "sfx" };
        private string[] _musicLabels = { "music" };
        private Addressables.MergeMode mergeMode = Addressables.MergeMode.Union;

        private const int SFX_3D_POOL_SIZE = 3;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<SignalSfxPlay>();
            Container.DeclareSignal<SignalMusicChange>();

            var rootGo = new GameObject("AudioRoot");
            Object.DontDestroyOnLoad(rootGo);
            var root = rootGo.transform;
            Container.Bind<Transform>()
                .WithId("AudioRoot")
                .FromInstance(root)
                .AsSingle();

            Container.Bind<SfxClipsProvider>()
                .AsSingle();
            Container.Bind<MusicClipsProvider>()
                .AsSingle();

            Container.BindInterfacesTo<SfxPlayer>()
                .AsSingle();
            Container.BindInterfacesTo<MusicPlayer>()
                .AsSingle();

            Container.Bind<int>()
                .WithId("Sfx3DPoolSize")
                .FromInstance(SFX_3D_POOL_SIZE);

            Container.BindInterfacesTo<AudioSignalHandler>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<AudioAssetsPreloader>()
                .AsSingle()
                .WithArguments(_sfxLabels, _musicLabels, mergeMode);
            
            Container.Bind<SceneMusicBootstrap>()
                .AsSingle();
        }
    }
}