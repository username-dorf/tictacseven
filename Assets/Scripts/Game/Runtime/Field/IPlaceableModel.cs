using System;
using UniRx;
using UnityEngine;

namespace Game.Field
{
    public interface IPlaceableModel
    {
        public interface ITransform: IDisposable
        {
            ReadOnlyReactiveProperty<Vector3> Position { get; }
            ReadOnlyReactiveProperty<Vector3> InitialPosition { get; }
            
            /// <summary>
            /// Indicates if it can be moved, usually after placement.
            /// </summary>
            ReadOnlyReactiveProperty<bool> Moveable { get; }
            ReadOnlyReactiveProperty<bool> IsSelected { get; }
            ReadOnlyReactiveProperty<bool> IsMoving { get; }
            
            void SetPosition(Vector3 position);
            void SetLocked(bool locked);
            void SetSelected(bool selected);
            void SetMoving(bool isMoving);
        }
        public interface IData: IDisposable
        {
            ReadOnlyReactiveProperty<int> Merit { get;}
            ReadOnlyReactiveProperty<int> Owner { get; }
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