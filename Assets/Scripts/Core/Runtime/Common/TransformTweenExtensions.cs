using PrimeTween;
using UnityEngine;

namespace Core.Common
{
    public static class TransformTweenExtensions
    {
        public static Sequence ScaleFromCorner(
            this Transform transform,
            Vector2Int anchor01,
            float duration = 0.5f,
            float spawnScaleFactor = 0.2f,
            float yBounce = 0.05f,
            float xzOvershootMul = 0.2f,
            float xzUndershootMul = 0.2f
        )
        {
            var baseScale = transform.localScale;
            var basePos = transform.localPosition;
            float baseX = baseScale.x, baseY = baseScale.y, baseZ = baseScale.z;


            Vector3 minL, maxL, centerL;
            var mf = transform.GetComponent<MeshFilter>();
            var smr = transform.GetComponent<SkinnedMeshRenderer>();
            var mr = transform.GetComponent<MeshRenderer>();

            if (mf != null && mf.sharedMesh != null)
            {
                var b = mf.sharedMesh.bounds;
                minL = b.min;
                maxL = b.max;
                centerL = b.center;
            }
            else if (smr != null)
            {
                var b = smr.localBounds;
                minL = b.min;
                maxL = b.max;
                centerL = b.center;
            }
            else if (mr != null)
            {
                var bw = mr.bounds;
                minL = transform.InverseTransformPoint(bw.min);
                maxL = transform.InverseTransformPoint(bw.max);
                centerL = (minL + maxL) * 0.5f;
            }
            else
            {
                minL = new Vector3(-0.5f, -0.5f, -0.5f);
                maxL = new Vector3(+0.5f, +0.5f, +0.5f);
                centerL = Vector3.zero;
            }

            float u = Mathf.Clamp01(anchor01.x);
            float v = Mathf.Clamp01(anchor01.y);
            var anchorLocal = new Vector3(
                Mathf.Lerp(minL.x, maxL.x, u),
                Mathf.Lerp(minL.y, maxL.y, v),
                Mathf.Lerp(minL.z, maxL.z, 0.5f)
            );

            Vector3 PosForScale(Vector3 targetScale)
            {
                Vector3 deltaS = new Vector3(baseX - targetScale.x, baseY - targetScale.y, baseZ - targetScale.z);
                Vector3 localDelta = new Vector3(deltaS.x * anchorLocal.x, deltaS.y * anchorLocal.y,
                    deltaS.z * anchorLocal.z);
                Vector3 worldDelta = transform.rotation * localDelta;
                return transform.parent
                    ? basePos + transform.parent.InverseTransformVector(worldDelta)
                    : basePos + worldDelta;
            }

            Vector3 s0 = new Vector3(baseX * spawnScaleFactor, baseY * spawnScaleFactor, baseZ * spawnScaleFactor);
            transform.localScale = s0;
            transform.localPosition = PosForScale(s0);

            float y1 = (yBounce > 0f) ? baseY * (1f - yBounce) : baseY;
            float y2 = (yBounce > 0f) ? baseY * (1f + yBounce * 0.55f) : baseY;
            float y3 = baseY;

            float xz1 = baseX * (1f + yBounce * xzOvershootMul);
            float xz2 = baseX * (1f - yBounce * xzUndershootMul);
            float xz3 = baseX;

            Vector3 s1 = new Vector3(xz1, y1, xz1);
            Vector3 s2 = new Vector3(xz2, y2, xz2);
            Vector3 s3 = new Vector3(xz3, y3, xz3);

            Vector3 p1 = PosForScale(s1);
            Vector3 p2 = PosForScale(s2);
            Vector3 p3 = basePos;

            float t1 = (yBounce > 0f) ? duration * 0.5f : duration;
            float t2 = (yBounce > 0f) ? duration * 0.25f : 0f;
            float t3 = Mathf.Max(0f, duration - (t1 + t2));

            var seq = Sequence.Create();

            if (yBounce > 0f)
            {
                seq.Group(Tween.ScaleX(transform, s1.x, t1, Ease.OutCubic));
                seq.Group(Tween.ScaleY(transform, s1.y, t1, Ease.OutCubic));
                seq.Group(Tween.ScaleZ(transform, s1.z, t1, Ease.OutCubic));
                seq.Group(Tween.LocalPosition(transform, p1, t1, Ease.OutCubic));

                seq.Chain(Tween.ScaleX(transform, s2.x, t2, Ease.InOutSine));
                seq.Group(Tween.ScaleY(transform, s2.y, t2, Ease.InOutSine));
                seq.Group(Tween.ScaleZ(transform, s2.z, t2, Ease.InOutSine));
                seq.Group(Tween.LocalPosition(transform, p2, t2, Ease.InOutSine));

                if (t3 > 0f)
                {
                    seq.Chain(Tween.ScaleX(transform, s3.x, t3, Ease.OutSine));
                    seq.Group(Tween.ScaleY(transform, s3.y, t3, Ease.OutSine));
                    seq.Group(Tween.ScaleZ(transform, s3.z, t3, Ease.OutSine));
                    seq.Group(Tween.LocalPosition(transform, p3, t3, Ease.OutSine));
                }
            }
            else
            {
                seq.Group(Tween.ScaleX(transform, s3.x, t1, Ease.OutCubic));
                seq.Group(Tween.ScaleY(transform, s3.y, t1, Ease.OutCubic));
                seq.Group(Tween.ScaleZ(transform, s3.z, t1, Ease.OutCubic));
                seq.Group(Tween.LocalPosition(transform, p3, t1, Ease.OutCubic));
            }

            return seq;
        }

        public static Sequence ScaleBounceAllAxes(
            this Transform transform,
            float duration = 0.5f,
            float squashY = 0.1f,
            float xzOvershootMul = 0.1f,
            float xzUndershootMul = 0.25f,
            float spawnScaleFactor = 0.8f
        )
        {
            var baseScale = transform.localScale;
            var basePos = transform.localPosition;
            float baseX = baseScale.x, baseY = baseScale.y, baseZ = baseScale.z;

            Vector3 centerLocal = Vector3.zero;
            var mf = transform.GetComponent<MeshFilter>();
            var smr = transform.GetComponent<SkinnedMeshRenderer>();
            var mr = transform.GetComponent<MeshRenderer>();

            if (mf != null && mf.sharedMesh != null)
            {
                centerLocal = mf.sharedMesh.bounds.center;
            }
            else if (smr != null)
            {
                centerLocal = smr.localBounds.center;
            }
            else if (mr != null)
            {
                centerLocal = transform.InverseTransformPoint(mr.bounds.center);
            }

            Vector3 PosForScale(Vector3 targetScale)
            {
                Vector3 deltaS = new Vector3(baseX - targetScale.x, baseY - targetScale.y, baseZ - targetScale.z);
                Vector3 localDelta = new Vector3(deltaS.x * centerLocal.x, deltaS.y * centerLocal.y,
                    deltaS.z * centerLocal.z);
                if (transform.parent != null)
                {
                    Vector3 worldDelta = transform.rotation * localDelta;
                    return basePos + transform.parent.InverseTransformVector(worldDelta);
                }
                else
                {
                    Vector3 worldDelta = transform.rotation * localDelta;
                    return basePos + worldDelta;
                }
            }

            Vector3 startScale =
                new Vector3(baseX * spawnScaleFactor, baseY * spawnScaleFactor, baseZ * spawnScaleFactor);
            transform.localScale = startScale;
            transform.localPosition = PosForScale(startScale);

            float y1 = baseY * (1f - squashY);
            float y2 = baseY * (1f + squashY * 0.55f);
            float y3 = baseY;

            float xz1 = baseX * (1f + squashY * xzOvershootMul);
            float xz2 = baseX * (1f - squashY * xzUndershootMul);
            float xz3 = baseX;

            Vector3 s1 = new Vector3(xz1, y1, xz1);
            Vector3 s2 = new Vector3(xz2, y2, xz2);
            Vector3 s3 = new Vector3(xz3, y3, xz3);

            Vector3 p1 = PosForScale(s1);
            Vector3 p2 = PosForScale(s2);
            Vector3 p3 = basePos;

            float t1 = duration * 0.5f;
            float t2 = duration * 0.25f;
            float t3 = Mathf.Max(0f, duration - (t1 + t2));

            var seq = Sequence.Create();
            seq.Group(Tween.ScaleX(transform, s1.x, t1, Ease.OutCubic));
            seq.Group(Tween.ScaleY(transform, s1.y, t1, Ease.OutCubic));
            seq.Group(Tween.ScaleZ(transform, s1.z, t1, Ease.OutCubic));
            seq.Group(Tween.LocalPosition(transform, p1, t1, Ease.OutCubic));

            seq.Chain(Tween.ScaleX(transform, s2.x, t2, Ease.InOutSine));
            seq.Group(Tween.ScaleY(transform, s2.y, t2, Ease.InOutSine));
            seq.Group(Tween.ScaleZ(transform, s2.z, t2, Ease.InOutSine));
            seq.Group(Tween.LocalPosition(transform, p2, t2, Ease.InOutSine));

            if (t3 > 0f)
            {
                seq.Chain(Tween.ScaleX(transform, s3.x, t3, Ease.OutSine));
                seq.Group(Tween.ScaleY(transform, s3.y, t3, Ease.OutSine));
                seq.Group(Tween.ScaleZ(transform, s3.z, t3, Ease.OutSine));
                seq.Group(Tween.LocalPosition(transform, p3, t3, Ease.OutSine));
            }

            return seq;
        }
    }

    // === те же утилиты, но public ===
    public struct MeshBoundsLocal
    {
        public Vector3 min, max, center;
    }

    public static class Buttons
    {
        public static MeshBoundsLocal GetLocalBounds(Transform t)
        {
            var mf = t.GetComponent<MeshFilter>();
            var smr = t.GetComponent<SkinnedMeshRenderer>();
            var mr = t.GetComponent<MeshRenderer>();
            if (mf != null && mf.sharedMesh != null)
            {
                var b = mf.sharedMesh.bounds;
                return new MeshBoundsLocal {min = b.min, max = b.max, center = b.center};
            }
            else if (smr != null)
            {
                var b = smr.localBounds;
                return new MeshBoundsLocal {min = b.min, max = b.max, center = b.center};
            }
            else if (mr != null)
            {
                var bmin = t.InverseTransformPoint(mr.bounds.min);
                var bmax = t.InverseTransformPoint(mr.bounds.max);
                return new MeshBoundsLocal {min = bmin, max = bmax, center = (bmin + bmax) * 0.5f};
            }
            else
            {
                return new MeshBoundsLocal
                {
                    min = new Vector3(-0.5f, -0.5f, -0.5f),
                    max = new Vector3(+0.5f, +0.5f, +0.5f),
                    center = Vector3.zero
                };
            }
        }

        public static Vector3 AnchorLocalFrom01(in MeshBoundsLocal b, Vector3 uvw01)
        {
            return new Vector3(
                Mathf.Lerp(b.min.x, b.max.x, Mathf.Clamp01(uvw01.x)),
                Mathf.Lerp(b.min.y, b.max.y, Mathf.Clamp01(uvw01.y)),
                Mathf.Lerp(b.min.z, b.max.z, Mathf.Clamp01(uvw01.z))
            );
        }

        // позиция для целевого скейла так, чтобы anchorLocal стоял на месте
        public static Vector3 PosForScaleKeepingAnchor(Transform t, Vector3 baseScale, Vector3 basePos,
            Vector3 anchorLocal, Vector3 targetScale)
        {
            Vector3 deltaS = baseScale - targetScale;
            Vector3 localDelta =
                new Vector3(deltaS.x * anchorLocal.x, deltaS.y * anchorLocal.y, deltaS.z * anchorLocal.z);
            Vector3 worldDelta = t.rotation * localDelta;
            if (t.parent != null)
                return basePos + t.parent.InverseTransformVector(worldDelta);
            return basePos + worldDelta;
        }

        // === Пресс/релиз ОТ ФИКСИРОВАННОЙ БАЗЫ ===

        public static Sequence PressBounceFromBase(
            this Transform button,
            Vector3 baseScale, Vector3 basePos, Vector3 anchorLocal,
            float duration = 0.18f,
            float pressAmount = 0.22f,
            float bounceBelow = 0.12f,
            float xzWiden = 0.08f
        )
        {
            float yPressed = baseScale.y * (1f - pressAmount);
            float yBelow = Mathf.Max(0.0001f, yPressed * (1f - bounceBelow));
            float xzPressed = baseScale.x * (1f + xzWiden);
            float xzBelow = baseScale.x * (1f + xzWiden * 1.15f);

            Vector3 s1 = new Vector3(xzBelow, yBelow, xzBelow);
            Vector3 s2 = new Vector3(xzPressed, yPressed, xzPressed);

            Vector3 p1 = PosForScaleKeepingAnchor(button, baseScale, basePos, anchorLocal, s1);
            Vector3 p2 = PosForScaleKeepingAnchor(button, baseScale, basePos, anchorLocal, s2);

            float t1 = duration * 0.58f;
            float t2 = duration - t1;

            var seq = Sequence.Create();
            seq.Group(Tween.ScaleX(button, s1.x, t1, Ease.OutCubic));
            seq.Group(Tween.ScaleY(button, s1.y, t1, Ease.OutCubic));
            seq.Group(Tween.ScaleZ(button, s1.z, t1, Ease.OutCubic));
            seq.Group(Tween.LocalPosition(button, p1, t1, Ease.OutCubic));

            if (t2 > 0f)
            {
                seq.Chain(Tween.ScaleX(button, s2.x, t2, Ease.OutSine));
                seq.Group(Tween.ScaleY(button, s2.y, t2, Ease.OutSine));
                seq.Group(Tween.ScaleZ(button, s2.z, t2, Ease.OutSine));
                seq.Group(Tween.LocalPosition(button, p2, t2, Ease.OutSine));
            }

            return seq;
        }

        public static Sequence ReleaseBounceToBase(
            this Transform button,
            Vector3 baseScale, Vector3 basePos, Vector3 anchorLocal,
            float duration = 0.22f,
            float reboundUp = 0.14f,
            float xzRelax = 0.06f
        )
        {
            // цель 1 (выплеск вверх) и цель 2 (база) считаются от ОДНОЙ базы
            float yAbove = baseScale.y * (1f + reboundUp);
            float xzNarrow = baseScale.x * (1f - xzRelax);
            Vector3 s1 = new Vector3(xzNarrow, yAbove, xzNarrow);
            Vector3 s2 = baseScale;

            Vector3 p1 = PosForScaleKeepingAnchor(button, baseScale, basePos, anchorLocal, s1);
            Vector3 p2 = PosForScaleKeepingAnchor(button, baseScale, basePos, anchorLocal, s2);

            float t1 = duration * 0.45f;
            float t2 = duration - t1;

            var seq = Sequence.Create();
            seq.Group(Tween.ScaleX(button, s1.x, t1, Ease.OutCubic));
            seq.Group(Tween.ScaleY(button, s1.y, t1, Ease.OutCubic));
            seq.Group(Tween.ScaleZ(button, s1.z, t1, Ease.OutCubic));
            seq.Group(Tween.LocalPosition(button, p1, t1, Ease.OutCubic));

            if (t2 > 0f)
            {
                seq.Chain(Tween.ScaleX(button, s2.x, t2, Ease.OutSine));
                seq.Group(Tween.ScaleY(button, s2.y, t2, Ease.OutSine));
                seq.Group(Tween.ScaleZ(button, s2.z, t2, Ease.OutSine));
                seq.Group(Tween.LocalPosition(button, p2, t2, Ease.OutSine));
            }

            return seq;
        }

        // Быстрый довод до pressed ОТ ФИКСИРОВАННОЙ БАЗЫ (для короткого тапа)
        public static Sequence QuickFinishToPressedFromBase(
            this Transform button,
            Vector3 baseScale, Vector3 basePos, Vector3 anchorLocal,
            float finishTime = 0.04f,
            float pressAmount = 0.22f,
            float xzWiden = 0.08f
        )
        {
            float yPressed = baseScale.y * (1f - pressAmount);
            float xzPressed = baseScale.x * (1f + xzWiden);

            Vector3 sPressed = new Vector3(xzPressed, yPressed, xzPressed);
            Vector3 pPressed = PosForScaleKeepingAnchor(button, baseScale, basePos, anchorLocal, sPressed);

            var seq = Sequence.Create();
            seq.Group(Tween.ScaleX(button, sPressed.x, finishTime, Ease.OutCubic));
            seq.Group(Tween.ScaleY(button, sPressed.y, finishTime, Ease.OutCubic));
            seq.Group(Tween.ScaleZ(button, sPressed.z, finishTime, Ease.OutCubic));
            seq.Group(Tween.LocalPosition(button, pPressed, finishTime, Ease.OutCubic));
            return seq;
        }
    }
}