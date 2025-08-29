using System;
using System.Collections.Generic;
using System.Threading;
using Core.AssetProvider;
using Core.Common;
using Cysharp.Threading.Tasks;
using Game.User;
using PrimeTween;
using UniRx;
using UnityEngine;

namespace Game.Entities
{
    public class EntitiesBackgroundView : MonoBehaviour
    {
        [field: SerializeField] public BoxCollider Collider { get; private set; }
        [field: SerializeField] public EntitiesBackgroundDebugView DebugView { get; private set; }
        [SerializeField] private Transform meshTransform;

        public async UniTask PlayScaleFromCorner(Vector2Int corner, CancellationToken ct)
        {
            try
            {
                await meshTransform.ScaleFromCorner(corner).ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        public class EntityPlaceholderViewsProvider
        {
            public Dictionary<int, EntityPlaceholderView> Placeholders { get; }

            public EntityPlaceholderViewsProvider(Dictionary<int, EntityView> placeholderViews)
            {
                Placeholders= new Dictionary<int, EntityPlaceholderView>();
                foreach (var kvp in placeholderViews)
                {
                    var placeHolderView = new EntityPlaceholderView(kvp.Value);
                    Placeholders.Add(kvp.Key, placeHolderView);
                }
            }

            public EntityPlaceholderView GetPlaceholder(int value)
            {
                var placeholder = Placeholders.GetValueOrDefault(value);
                if (placeholder == null)
                    throw new Exception($"Placeholder {value} not found");
                return placeholder;
            }
        }
        public class EntityPlaceholderView
        {
            public EntityView View { get; private set; }

            private readonly Vector3 _initPosition;
            private readonly Vector3 _selectedOffset = new Vector3(0f, 0.25f, 0f);
            private readonly float _duration = 0.25f;

            private CancellationTokenSource _cts;
            private Tween _tween;
            private bool _isSelected;

            public EntityPlaceholderView(EntityView view)
            {
                View = view;
                _initPosition = view.transform.position;
                _isSelected = false;
            }

            public async UniTask DoSelect(bool isSelected, CancellationToken token = default) {
                if (isSelected == _isSelected) return;

                if(_cts is not null && !_cts.IsCancellationRequested)
                    _cts.Cancel();
                _cts?.Dispose();
                _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                var ct = _cts.Token;

                if (_tween.isAlive) 
                    _tween.Stop();

                var from = View.transform.position;
                var to = isSelected ? _initPosition + _selectedOffset : _initPosition;

                var isRelease = (to - _initPosition).sqrMagnitude <= 1e-8f;
                var duration = isRelease ? 0f : _duration;
                _isSelected = isSelected;

                if ((to - from).sqrMagnitude <= 1e-8f || duration <= 0f) {
                    SetPosition(to);
                    return;
                }

                try {
                    _tween = Tween.Custom(from, to, duration, SetPosition);
                    await _tween.ToUniTask(cancellationToken: ct);
                }
                catch (OperationCanceledException) {
                    if (token.IsCancellationRequested) throw;
                }
            }

            public void SetSelected(bool isSelected)
            {
                if(_cts is not null && !_cts.IsCancellationRequested)
                    _cts.Cancel();
                _cts?.Dispose();
                
                if (_tween.isAlive) 
                    _tween.Stop();
                
                _isSelected = isSelected;
                if (View)
                    View.transform.position = _initPosition;
            }

            private void SetPosition(Vector3 p)
            {
                if (View) 
                    View.transform.position = p;
            }

            public void SetValueSprite(Sprite sprite)
            {
                View.ChangeValueOnMaterial(sprite);
            }
        }
        public class EntitiesPlaceholderViewFactory : AssetProvider<EntityView>
        {
            private readonly EntitiesValueSpriteProvider _spriteProvider;
            private const string ASSET_KEY = "Entity_Placeholder_tiled";

            public EntitiesPlaceholderViewFactory(EntitiesValueSpriteProvider spriteProvider)
            {
                _spriteProvider = spriteProvider;
            }
            public async UniTask<EntitiesPlaceholderPresenter> Create(UserEntitiesModel userEntitiesModel, CancellationToken token)
            {
                await LoadAsset(token, ASSET_KEY);
                var prefab = GetAsset();
                var dict = new Dictionary<int, EntityView>();
                foreach (var placeableModel in userEntitiesModel.Entities)
                {
                    var position = placeableModel.Transform.InitialPosition.Value+new Vector3(0,-1f,0);
                    var placeholder = Instantiate(prefab, position, Quaternion.identity);
                    placeholder.transform.localScale = Vector3.one*0.8f;
                    dict.Add(placeableModel.Data.Merit.Value, placeholder);
                }

                var placeholderViews = new EntityPlaceholderViewsProvider(dict);
                var presenter = new EntitiesPlaceholderPresenter(userEntitiesModel, placeholderViews, _spriteProvider);
                return presenter;
            }
        }
        public class EntitiesPlaceholderPresenter: IDisposable
        {
            private CompositeDisposable _disposable;
            private readonly EntitiesValueSpriteProvider _spriteProvider;
            private readonly EntityPlaceholderViewsProvider _viewsProvider;

            public EntitiesPlaceholderPresenter(
                UserEntitiesModel userEntitiesModel,
                EntityPlaceholderViewsProvider viewsProvider,
                EntitiesValueSpriteProvider spriteProvider)
            {
                _viewsProvider = viewsProvider;
                _spriteProvider = spriteProvider;
                _disposable = new CompositeDisposable();
                foreach (var (key, value) in _viewsProvider.Placeholders)
                {
                    value.SetValueSprite(_spriteProvider.GetAsset(key));
                }
                
                foreach (var placeableModel in userEntitiesModel.Entities)
                {
                    placeableModel.Transform.IsSelected
                        .Skip(1)
                        .Subscribe(isSelected =>
                        {
                            _viewsProvider.GetPlaceholder(placeableModel.Data.Merit.Value)
                                .DoSelect(isSelected || !placeableModel.Transform.IsMoveable.Value).Forget();
                        })
                        .AddTo(_disposable);

                    placeableModel.Transform.Visible
                        .Where(x=>!x)
                        .Subscribe(_ => DisablePlaceholder(_viewsProvider.GetPlaceholder(placeableModel.Data.Merit.Value)))
                        .AddTo(_disposable);
                }
            }

            public void Drop()
            {
                foreach (var (key, value) in _viewsProvider.Placeholders)
                {
                    value.SetValueSprite(_spriteProvider.GetAsset(key));
                    value.SetSelected(false);
                }
            }

            private void DisablePlaceholder(EntityPlaceholderView view)
            {
                view.SetValueSprite(_spriteProvider.GetAsset(0));
            }

            public void Dispose()
            {
                _disposable?.Dispose();
            }
        }
    }
}