using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Entities
{
    public class EntityView : MonoBehaviour
    {
        [field: SerializeField] public EntityDebugView DebugView { get; private set; }
        [SerializeField] private BoxCollider collider;
        private EntityViewModel _viewModel;
        private float _maxRayDistance = 100f;


        public void Initialize(EntityViewModel viewModel)
        {
            _viewModel = viewModel;

            viewModel.Value
                .Subscribe(OnValueChanged)
                .AddTo(this);

            viewModel.Position
                .Subscribe(OnPositionChanged)
                .AddTo(this);
        }

        private void Update()
        {
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
                    _viewModel.SetDragging(true);
                }
                else
                {
                    _viewModel.SetSelected(false);
                }
            }

            if (_viewModel.IsDragging.Value && isPressed && _viewModel.DragPlane.HasValue)
            {
                if (_viewModel.DragPlane.Value.Raycast(ray, out float enter))
                {
                    Vector3 worldPos = ray.GetPoint(enter);
                    _viewModel.SetPosition(worldPos);
                }
            }

            if (pressUp)
            {
                _viewModel.SetSelected(false);
                _viewModel.SetDragging(false);
            }
        }

        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }

        private void OnValueChanged(int value)
        {
            DebugView.SetValue(value);
        }

        private void OnPositionChanged(Vector3 position)
        {
            transform.position = position;
        }
    }
}