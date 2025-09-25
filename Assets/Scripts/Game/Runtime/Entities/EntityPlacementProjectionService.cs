using System;
using System.Collections.Generic;
using System.Threading;
using Game.Entities.VFX;
using Game.Field;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Entities
{
    public interface IEntityPlacementProjectionService
    {
        void Initialize(Vector3[,] cellPositions, List<Vector3>[,] cellEdges, FieldModel fieldModel);
        void Clear();
    }
    public interface IEntityPlacementProjectionRegistrar
    {
        void RegisterEntity(EntityViewModel viewModel);
    }
    
    public class EntityPlacementProjectionService : IEntityPlacementProjectionService, IEntityPlacementProjectionRegistrar, IDisposable
    {
        private readonly EntityProjectionFXPool _fxPool;
        private FieldModel _fieldModel;
        private Vector3[,] _cellPositions;
        private List<Vector3>[,] _cellEdges;
        
        private CompositeDisposable _disposable;
        private CancellationTokenSource _cts;
        
        private Vector2Int _projectionCoors = new(-1,-1);

        public EntityPlacementProjectionService(EntityProjectionFXPool fxPool)
        {
            _fxPool = fxPool;
            _disposable = new CompositeDisposable();
        }
        
        public void Initialize(Vector3[,] cellPositions, List<Vector3>[,] cellEdges, FieldModel fieldModel)
        {
            _fieldModel = fieldModel ?? throw new ArgumentNullException(nameof(fieldModel));
            _cellPositions = cellPositions;
            _cellEdges = cellEdges;
        }
        
        public void RegisterEntity(EntityViewModel viewModel)
        {
            viewModel.Position
                .Where(_ => viewModel.IsMoving.Value)
                .Subscribe(OnMoving)
                .AddTo(_disposable);

            viewModel.IsMoving
                .Where(x => !x)
                .Skip(1)
                .Subscribe(_ =>ClearNearestProjection())
                .AddTo(_disposable);

        }

        public void Clear()
        {
            _cellPositions = null;
            _cellEdges = null;
            _disposable.Clear();
        }

        private void OnMoving(Vector3 position)
        {
            if(!CanProject())
                return;

            try
            {
                var nearestCoors = _cellPositions.FindNearestPlace(position);
                if (!CanProjectOn(nearestCoors))
                {
                    ClearNearestProjection();
                    return;
                }
                
                if (_projectionCoors.x != nearestCoors.x || _projectionCoors.y != nearestCoors.y)
                {
                    if (_cts is not null && !_cts.IsCancellationRequested)
                        _cts?.Cancel();
                    _cts = new CancellationTokenSource();
                    _projectionCoors = nearestCoors;

                    var path = _cellEdges[nearestCoors.x, nearestCoors.y];
                    var task = _fxPool.PlayAlongPathAsync(path, Vector3.up, 1f, null, 8, true, _cts.Token);
                }
            }
            catch (InvalidOperationException)
            {
                ClearNearestProjection();
            }
        }
        private void ClearNearestProjection()
        {
            _projectionCoors = new Vector2Int(-1, -1);
            if (_cts is not null && !_cts.IsCancellationRequested)
                _cts?.Cancel();
        }

        private bool CanProject()
        {
            return _cellEdges is not null && _cellPositions is not null;
        }

        private bool CanProjectOn(Vector2Int coors)
        {
            return _fieldModel.IsEmpty(coors);
        }

        public void Dispose()
        {
            Clear();
            if(_cts is not null && !_cts.IsCancellationRequested)
                _cts?.Cancel();
            _cts?.Dispose();
            _disposable?.Clear();
        }
    }
}