using System;
using UniRx;
using UnityEngine;

namespace Game.Field
{
    public interface IPlaceableModel
    {
        public interface ITransform: IDisposable
        {
            ReadOnlyReactiveProperty<bool> Visible { get; }
            IReadOnlyReactiveProperty<Vector3> Position { get; }
            IReadOnlyReactiveProperty<Vector3> Scale { get; }
            Vector3 InitialPosition { get; }
            Vector3 InitialScale { get; }
            
            /// <summary>
            /// Indicates if it can be moved, usually after placement.
            /// </summary>
            ReadOnlyReactiveProperty<bool> IsMoveable { get; }
            ReadOnlyReactiveProperty<bool> IsSelected { get; }
            ReadOnlyReactiveProperty<bool> Interactable { get; }
            ReadOnlyReactiveProperty<bool> IsMoving { get; }
            
            void SetVisible(bool visible);
            void SetPosition(Vector3 position);
            void SetMoveable(bool isMoveable);
            void SetSelected(bool selected);
            void SetMoving(bool isMoving);
            void SetInteractable(bool isSelectable);
            void SetScale(Vector3 scale);
            void Reset();
        }
        public interface IData: IDisposable
        {
            IReadOnlyReactiveProperty<int> Merit { get;}
            IReadOnlyReactiveProperty<int> Owner { get; }
        }
        public interface IEvents : IDisposable
        {
            ReactiveCommand<IPlaceableModel> ReleaseCommand { get; }
            ReactiveCommand<IPlaceableModel> ReleaseApprovedCommand { get; }
        }
        
        IData Data { get; }
        ITransform Transform { get; }
        IEvents Events { get; }
    }
}