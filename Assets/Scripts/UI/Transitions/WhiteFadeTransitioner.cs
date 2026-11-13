using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WhiteFadeTransitioner : Transitioner
{
    [Header("Implementation Config")]
    [SerializeField] Image image;
    [SerializeField] Ease easeIn;
    [SerializeField] Ease easeOut;

    protected override float PerformTransition(float time, bool backwards = false)
    {
        base.PerformTransition(time, backwards);
        OnTransitionStart();

        // Set values depending on direction
        var (start, target, ease) = backwards
            ? (1f, 0f, easeOut)
            : (0f, 1f, easeIn);

        image.color = new(1, 1, 1, start);
        image.DOKill();

        // Execute transition
        image
            .DOFade(target, time)
            .SetEase(ease)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(OnTransitionFinish);

        return time;
    }

    public override void Reset()
    {
        base.Reset();
        image.color = new(1, 1, 1, 0);
    }
}
