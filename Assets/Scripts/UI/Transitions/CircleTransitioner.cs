using DG.Tweening;
using UnityEngine;

public class CircleTransitioner : Transitioner
{
    [Header("Implementation Config")]

    [SerializeField] float openRadius = 0.6f;
    [SerializeField] float closeRadius = 0f;
    [SerializeField] Ease easeIn;
    [SerializeField] Ease easeOut;

    [Header("References")]
    [SerializeField] Material material;

    const string RADIUS_PROPERTY = "_Radius";

    protected override void Awake()
    {
        base.Awake();

        // Start as open by default
        material.SetFloat(RADIUS_PROPERTY, openRadius);
    }

    protected override float PerformTransition(float time, bool backwards = false)
    {
        base.PerformTransition(time, backwards);
        OnTransitionStart();

        // Set values depending on direction
        float start = backwards ? closeRadius : openRadius;
        float target = backwards ? openRadius : closeRadius;
        Ease ease = backwards ? easeOut : easeIn;

        material.SetFloat(RADIUS_PROPERTY, start);

        material.DOKill();
        material
            .DOFloat(target, RADIUS_PROPERTY, time)
            .SetEase(ease)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(OnTransitionFinish);

        return time;
    }

    public override void Reset()
    {
        base.Reset();
        material.SetFloat(RADIUS_PROPERTY, openRadius);
    }
}