using System;
using PrimeTween;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public static class UITweenExtensions
{
    public static async UniTask ScaleWithPinnedPoint(
        RectTransform rt,
        float targetScale,
        float duration,
        Vector2 pinNormalized,
        Ease ease = Ease.OutCubic,
        CancellationToken ct = default)
    {
        var pin = pinNormalized;

        Vector3 localPin =
            new Vector3((pin.x - rt.pivot.x) * rt.rect.width,
                (pin.y - rt.pivot.y) * rt.rect.height,
                0f);

        Vector3 pinWorld = rt.TransformPoint(localPin);

        Vector3 originalScale = rt.localScale;
        var originalPosition = rt.localPosition;

        rt.localScale = new Vector3(targetScale, targetScale, originalScale.z);
        Vector3 pinOffsetWorldAfter = rt.TransformVector(localPin);
        Vector3 targetPivotWorld = pinWorld - pinOffsetWorldAfter;
        rt.localScale = originalScale;

        var tScale = Tween.Scale(rt, new Vector3(targetScale, targetScale, originalScale.z), duration, ease);
        var tPos = Tween.Position(rt, targetPivotWorld, duration, ease);

        try
        {
            await Sequence.Create()
                .Group(tScale)
                .Group(tPos)
                .WithCancellation(ct);
        }
        catch (OperationCanceledException)
        {
            rt.localScale = originalScale;
            rt.localPosition = originalPosition;
        }
    }

    public static UniTask ScaleKeepRight(
        this RectTransform rt,
        float targetScale,
        float duration,
        Ease ease = Ease.OutCubic,
        CancellationToken ct = default)
    {
        return ScaleWithPinnedPoint(rt, targetScale, duration, new Vector2(1f, 0.5f), ease, ct);
    }

    public static UniTask ScaleKeepLeft(
        this RectTransform rt,
        float targetScale,
        float duration,
        Ease ease = Ease.OutCubic,
        CancellationToken ct = default)
    {
        return ScaleWithPinnedPoint(rt, targetScale, duration, new Vector2(0f, 0.5f), ease, ct);
    }
}