using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public static class TransformExtensions
{
    public static async UniTask MoveAsync(this Transform transform, AnimationCurve curve, Vector3 position,
        CancellationToken ct, bool rotateTowards = false, float rotationSpeed = 0f)
    {
        var duration = Vector3.Distance(transform.position, position);

        var moveTween = transform
            .DOMove(position, duration)
            .SetEase(Ease.Linear);

        async UniTask WatchMoveAsync()
        {
            try
            {
                while (moveTween.IsActive() && !moveTween.IsComplete())
                {
                    ct.ThrowIfCancellationRequested();

                    var t = moveTween.ElapsedPercentage();
                    moveTween.timeScale = curve.Evaluate(t);

                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }
            catch (OperationCanceledException)
            {
                moveTween.Kill();
                throw;
            }
        }

        if (rotateTowards)
        {
            await UniTask.WhenAll(
                WatchMoveAsync(),
                transform.DOLookAt(position, rotationSpeed)
                    .SetSpeedBased(true)
                    .AsyncWaitForCompletion()
                    .AsUniTask()
            );
        }
        else
        {
            await WatchMoveAsync();
        }
    }
}
