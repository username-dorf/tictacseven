using System;
using Core.Common;
using Game.Field;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Entities
{
    public class EntityView : MaterialApplicableView
    {
        [field: SerializeField] public EntityDebugView DebugView { get; private set; }
        [SerializeField] private BoxCollider collider;
        private float _maxRayDistance = 100f;
        private bool _enableScaleAnimation = true;
        private Vector3 _animationInitialScale;
        private Vector3 _animationInitialPosition;
        
        private EntityViewModel _viewModel;
        private FieldViewProvider _fieldViewProvider;


        public void Initialize(EntityViewModel viewModel, FieldViewProvider fieldViewProvider)
        {
            _fieldViewProvider = fieldViewProvider;
            _viewModel = viewModel;

            viewModel.Value
                .Subscribe(OnValueChanged)
                .AddTo(this);

            viewModel.Position
                .Subscribe(OnPositionChanged)
                .AddTo(this);
            
            viewModel.Scale
                .Subscribe(OnScaleChanged)
                .AddTo(this);
            
            viewModel.Material
                .Subscribe(OnMaterialChanged)
                .AddTo(this);

            viewModel.ValueSprite
                .Subscribe(ChangeValueOnMaterial)
                .AddTo(this);

            viewModel.IsVisible
                .Subscribe(OnVisibleChanged)
                .AddTo(this);
            
            _animationInitialScale = transform.localScale;
            _animationInitialPosition = transform.position;
        }

        private void Update()
        {
            if(_viewModel is not null && _viewModel.IsInteractable.Value)
                HandlePointerInput();
        }

        private void HandlePointerInput()
        {
            Vector2 screenPos = default;
            bool pressDown = false, pressUp = false, isPressed = false;

            if (Touchscreen.current != null)
            {
                var t = Touchscreen.current.primaryTouch;
                screenPos = t.position.ReadValue();
                pressDown = t.press.wasPressedThisFrame;
                pressUp   = t.press.wasReleasedThisFrame;
                isPressed = t.press.isPressed;
            }

            if (Mouse.current != null)
            {
                if (!isPressed)
                    screenPos = Mouse.current.position.ReadValue();
                pressDown |= Mouse.current.leftButton.wasPressedThisFrame;
                pressUp   |= Mouse.current.leftButton.wasReleasedThisFrame;
                isPressed |= Mouse.current.leftButton.isPressed;
            }

            if (!pressDown && !pressUp && !isPressed)
                return;

            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (pressDown)
            {
                if (Physics.Raycast(ray, out var hit, _maxRayDistance) && hit.collider == collider)
                {
                    _viewModel.SetSelected(true);
                    _viewModel.SetMoving(true);
                }
                else
                {
                    _viewModel.SetSelected(false);
                }
            }

            var dragPlane = _fieldViewProvider.View.DragPlane;
            if (_viewModel.IsMoving.Value && isPressed)
            {
                if (dragPlane.Raycast(ray, out float enter))
                {
                    Vector3 worldPos = ray.GetPoint(enter);
                    _viewModel.SetPosition(worldPos);
                }
            }

            if (pressUp)
            {
                _viewModel.SetSelected(false);
                _viewModel.SetMoving(false);
            }
        }

        private void OnScaleChanged(Vector3 scale)
        {
            transform.localScale = scale;
        }

        private void OnValueChanged(int value)
        {
            DebugView.SetValue(value);
        }

        private void OnPositionChanged(Vector3 position)
        {
            transform.position = position;
            if(_enableScaleAnimation)
                DoScaleRelativeToPosition(position,_animationInitialPosition, _animationInitialScale);
        }
        private void OnMaterialChanged(Material material)
        {
            ChangeMaterial(material);
        }
        public void ChangeValueOnMaterial(Sprite sprite)
        {
            var mpb = new MaterialPropertyBlock();
            Renderer.GetPropertyBlock(mpb);
            mpb.SetTexture("_DigitTex", sprite.texture);
            Renderer.SetPropertyBlock(mpb);
        }
        private void OnVisibleChanged(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        private void DoScaleRelativeToPosition(Vector3 position, Vector3 initPosition, Vector3 initScale,
            float maxDistance = 3, float maxScale = 1.0f)
        {
            position = new Vector3(position.x, 0, position.z);
            float d = Vector3.Distance(position,new Vector3(initPosition.x,0,initPosition.z));
            float t = (maxDistance > 0f) ? Mathf.Clamp01(d / maxDistance) : 1f;
            transform.localScale=Vector3.Lerp(initScale, Vector3.one * maxScale, t);
        }
    }
}