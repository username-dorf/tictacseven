using System;
using System.Collections.Generic;
using Core.User;
using UniRx;
using UnityEngine;
using Zenject;

namespace Core.UI.Components
{
    public class UIProfileSelectorView: UISelectorView
    {
        [SerializeField] private UIProfileView profileView;

        public void Initialize(IObservable<Sprite> profileSprite, Action onNext, Action onPrevious)
        {
            profileView.Initialize(profileSprite);
            base.Initialize(onNext, onPrevious);
        }
    }
    
    public class UIProfileSelectorPresenter : IDisposable
    {
        private UIProfileSelectorView _view;
        private IUserPreferencesProvider _userPreferencesProvider;
        private ProfileSpriteSetsProvider _profileSpritesProvider;

        private ProfileEmotion _emotion = ProfileEmotion.Default;
        private int _currentProfileIndex;

        private List<string> _allProfileIds;
        private ReactiveProperty<Sprite> _currentProfileSprite;

        public UIProfileSelectorPresenter(
            UIProfileSelectorView view,
            [Inject] IUserPreferencesProvider userPreferencesProvider,
            [Inject] ProfileSpriteSetsProvider profileSpritesProvider)
        {
            _profileSpritesProvider = profileSpritesProvider;
            _userPreferencesProvider = userPreferencesProvider;
            _view = view;
        }

        public void Initialize()
        {
            var allAssets = _profileSpritesProvider.GetAllAssets();
            if (allAssets is null or { Count: 0 })
                throw new Exception("Assets dictionary is null or empty");
            _allProfileIds = new List<string>(allAssets.Keys);
            
            var userPreferences = _userPreferencesProvider.Current;
            var userProfileAssetId = userPreferences.ProfileAssetId.Value;
            var userProfileSprites = _profileSpritesProvider.GetAsset(userProfileAssetId);
            _currentProfileIndex = _allProfileIds.IndexOf(userProfileAssetId);
            _currentProfileSprite = new ReactiveProperty<Sprite>(userProfileSprites.GetEmotionSprite(_emotion));
            
            _view.Initialize(_currentProfileSprite,ChangeToNextSprite, ChangeToPreviousSprite);
        }

        private void ChangeToNextSprite()
        {
            var maxIndex = _allProfileIds.Count;
            var nextProfileIndex = _currentProfileIndex + 1;
            if(nextProfileIndex>=maxIndex)
                nextProfileIndex = 0;
            ChangeCurrentSprite(_allProfileIds[nextProfileIndex]);
        }

        private void ChangeToPreviousSprite()
        {
            var previousIndex = _currentProfileIndex - 1;
            if(previousIndex<0)
                previousIndex = _allProfileIds.Count - 1;
            ChangeCurrentSprite(_allProfileIds[previousIndex]);
        }
        
        private void ChangeCurrentSprite(string id)
        {
            _userPreferencesProvider.Current.ProfileAssetId.Value = id;
            _currentProfileIndex = _allProfileIds.IndexOf(id);
            _currentProfileSprite.Value = _profileSpritesProvider.GetAsset(id).GetEmotionSprite(_emotion);
        }

        public void Dispose()
        {
            _currentProfileSprite?.Dispose();
        }
    }
}