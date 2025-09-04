using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Field;
using PrimeTween;
using UnityEngine;

namespace Game.User
{
    public interface IEntitiesController
    {
        UniTask DoMoveAsync(int value, Vector2Int coors, CancellationToken token, float duration = 0.5f);
    }

    public class UserEntitiesController : IEntitiesController
    {
        private FieldModel _fieldModel;
        private UserEntitiesModel _entitiesModel;
        private FieldViewProvider _fieldViewProvider;

        public UserEntitiesController(FieldModel fieldModel, UserEntitiesModel entitiesModel,
            FieldViewProvider fieldViewProvider)
        {
            _fieldViewProvider = fieldViewProvider;
            _entitiesModel = entitiesModel;
            _fieldModel = fieldModel;
        }

        public async UniTask DoMoveAsync(int value, Vector2Int coors, CancellationToken token, float duration = 0.5f)
        {
            var movableEntity = _entitiesModel.Entities.FirstOrDefault(x => x.Data.Merit.Value == value);
            if (movableEntity is null)
                return;

            var fieldPosition = _fieldModel.Entities[coors].Transform.Position.Value;
            var entityStartPosition = movableEntity.Transform.Position.Value;

            if ((fieldPosition - entityStartPosition).sqrMagnitude <= 1e-8f)
                return;

            var dragPlane = _fieldViewProvider.View.DragPlane;
            var n = dragPlane.normal.normalized;

            var startOnPlane = dragPlane.ClosestPointOnPlane(entityStartPosition);
            var targetOnPlane = dragPlane.ClosestPointOnPlane(fieldPosition);

            var hStart = Vector3.Dot(entityStartPosition - startOnPlane, n);
            var hTarget = Vector3.Dot(fieldPosition - targetOnPlane, n);

            var afterLift = startOnPlane + n * hTarget;
            var finalPos = targetOnPlane + n * hTarget;

            var dHeight = Mathf.Abs(hTarget - hStart);
            var dLateral = Vector3.Distance(startOnPlane, targetOnPlane);
            var total = dHeight + dLateral;
            var durLift = total > 1e-5f ? duration * (dHeight / total) : 0f;
            var durMove = total > 1e-5f ? duration * (dLateral / total) : 0f;

            try
            {
                movableEntity.Transform.SetSelected(true);

                if (durLift > 0f)
                {
                    await Tween.Custom(entityStartPosition, afterLift, durLift, pos =>
                        {
                            var clamped = dragPlane.ClosestPointOnPlane(pos);
                            movableEntity.Transform.SetPosition(clamped + n * hTarget);
                        })
                        .WithCancellation(token);
                }
                else
                {
                    movableEntity.Transform.SetPosition(afterLift);
                }

                token.ThrowIfCancellationRequested();

                if (durMove > 0f)
                {
                    await Tween.Custom(afterLift, finalPos, durMove, pos =>
                        {
                            var clamped = dragPlane.ClosestPointOnPlane(pos);
                            movableEntity.Transform.SetPosition(clamped + n * hTarget);
                        })
                        .WithCancellation(token);
                }

                movableEntity.Transform.SetPosition(fieldPosition);
                movableEntity.Transform.SetSelected(false);
            }
            catch (OperationCanceledException)
            {
                movableEntity.Transform.SetPosition(entityStartPosition);
                movableEntity.Transform.SetSelected(false);
            }
        }
    }
}