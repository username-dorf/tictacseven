using System;
using System.Collections.Generic;
using System.Threading;
using Core.Common;
using Core.Data;
using Core.User;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI.Components
{
    public class UIEntitySkinSelectorView : UISelectorView
    {
        [field: SerializeField] public UIEntitySkinView EntitySkinView { get; private set; }
    }

    public class UIEntitySkinSelectorPresenter : IDisposable
    {
        private SkinMaterialAssetsProvider _skinMaterialAssetsProvider;
        private IUserPreferencesProvider _userPreferencesProvider;
        private SkinPreviewCamera _previewCamera;
        private UIEntitySkinView.Factory _entitySkinViewFactory;
        
        private List<MaterialId> _allAvailableMaterials;
        private int _currentMaterialIndex;
        
        private UIEntitySkinSelectorView _view;

        public UIEntitySkinSelectorPresenter(
            UIEntitySkinSelectorView view,
            IUserPreferencesProvider userPreferencesProvider,
            SkinMaterialAssetsProvider skinMaterialAssetsProvider,
            UIEntitySkinView.Factory entitySkinViewFactory)
        {
            _view = view;
            _entitySkinViewFactory = entitySkinViewFactory;
            _userPreferencesProvider = userPreferencesProvider;
            _skinMaterialAssetsProvider = skinMaterialAssetsProvider;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            var allAssets = _skinMaterialAssetsProvider.GetAllAssets();
            if (allAssets is null or {Count: 0})
                throw new Exception("Assets dictionary is null or empty");
            _allAvailableMaterials = new List<MaterialId>(allAssets.Keys);
            _allAvailableMaterials.Remove(MaterialId.Opponent); //todo setup elsewhere
            
            var materialId = _userPreferencesProvider.Current.TileMaterialId.Value;
            var material = _skinMaterialAssetsProvider.Get(materialId);
            _currentMaterialIndex = _allAvailableMaterials.IndexOf(materialId);
            (_, _previewCamera) = await _entitySkinViewFactory.BindExisting(_view.EntitySkinView, material, ct);
            _view.Initialize(ChangeToNext, ChangeToPrevious);
        }

        private void ChangeToNext()
        {
            var maxIndex = _allAvailableMaterials.Count;
            var nextIndex = _currentMaterialIndex + 1;
            if(nextIndex>=maxIndex)
                nextIndex = 0;
            ChangeMaterial(_allAvailableMaterials[nextIndex]);
        }

        private void ChangeToPrevious()
        {
            var previousIndex = _currentMaterialIndex - 1;
            if(previousIndex<0)
                previousIndex = _allAvailableMaterials.Count - 1;
            ChangeMaterial(_allAvailableMaterials[previousIndex]);
        }

        private void ChangeMaterial(MaterialId materialId)
        {
            _userPreferencesProvider.Current.TileMaterialId.Value = materialId;
            _currentMaterialIndex = _allAvailableMaterials.IndexOf(materialId);
            var material = _skinMaterialAssetsProvider.Get(materialId);
            _previewCamera.PreviewView.ChangeMaterial(material);
            _previewCamera.UpdateRender();
        }

        public void Dispose()
        {
        }
    }
}