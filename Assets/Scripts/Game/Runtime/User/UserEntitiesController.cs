using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Field;
using PrimeTween;
using UnityEngine;

namespace Game.User
{
    public class UserEntitiesController
    {
        private FieldModel _fieldModel;
        private UserEntitiesModel _entitiesModel;

        public UserEntitiesController( UserEntitiesModel entitiesModel, FieldModel fieldModel)
        {
            _entitiesModel = entitiesModel;
            _fieldModel = fieldModel;
        }

        public async UniTask DoMoveAsync(int value, Vector2Int coors, CancellationToken token)
        {
            var canMove = _fieldModel.Entities[coors].IsEmptyOwner();
            if(!canMove)
                return;
            
            var movableEntity = _entitiesModel.Entities.FirstOrDefault(x => x.Data.Merit.Value == value);
            if(movableEntity is null)
                return;
            
            var fieldPosition = _fieldModel.Entities[coors].Transform.Position.Value;
            var entityStartPosition = movableEntity.Transform.Position.Value;
            try
            {
                await Tween.Custom(entityStartPosition, fieldPosition, 0.5f, movableEntity.Transform.SetPosition)
                    .OnComplete(() => movableEntity.Events.ReleaseCommand.Execute(movableEntity))
                    .WithCancellation(token);
            }
            catch (Exception e)
            {
                if(e is OperationCanceledException)
                    movableEntity.Transform.SetPosition(entityStartPosition);
            }
        }
    }
}