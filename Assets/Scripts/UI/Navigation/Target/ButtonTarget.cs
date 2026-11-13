using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonTarget : NavigationTarget
{
    [field: Space]
    [field: SerializeField] public UnityEvent OnClick { get; private set; }

    Image image;

    readonly Vector2 animationTimeInOut = new(0.067f, 0.1f);
    readonly Vector2 scaleMinMax = new(0.9f, 1.05f);
    readonly float scaleTimeFactor = 2.2f;
    readonly float colorLoopTime = 0.6f;

    Coroutine colorLoopCoroutine;
    StaticConfig.UIColors_ uIColors;

    protected override void Awake()
    {
        base.Awake();
        image = GetComponent<Image>();
        uIColors = GameManager.I.StaticConfig.UIColors;
    }

    public override void Accept()
    {
        base.Accept();
        OnClick?.Invoke();
    }

    public override void EnableSelectedVisuals()
    {
        base.EnableSelectedVisuals();
        AnimateSelected(true);
    }

    public override void DisableSelectedVisuals()
    {
        base.DisableSelectedVisuals();
        AnimateSelected(false);
    }

    void AnimateSelected(bool isBeingSelected)
    {
        if (isBeingSelected)
        {
            StopSelectedColorLoop();
            colorLoopCoroutine = StartCoroutine(SelectedColorLoop());

            float targetScale = NavigationSystem.I.CurrentHoldingButton == this ? scaleMinMax.x : scaleMinMax.y;
            AnimateScale(targetScale, animationTimeInOut.x * scaleTimeFactor, Ease.OutSine);
        }
        else
        {
            AnimateColor(uIColors.Default, animationTimeInOut.y, Ease.InSine);
            StopSelectedColorLoop();
            AnimateScale(1, animationTimeInOut.y * scaleTimeFactor, Ease.InSine);
        }
    }

    void AnimateColor(Color target, float time, Ease easing)
    {
        image.DOKill();
        image
            .DOColor(target, time)
            .SetEase(easing)
            .SetUpdate(UpdateType.Normal, true);
    }

    void AnimateScale(float target, float time, Ease easing)
    {
        RectTransform.DOKill();
        RectTransform
            .DOScale(target, time)
            .SetEase(easing)
            .SetUpdate(UpdateType.Normal, true);
    }

    IEnumerator SelectedColorLoop()
    {
        Ease easing = Ease.InOutSine;
        AnimateColor(uIColors.Selected, animationTimeInOut.x, easing);
        yield return new WaitForSecondsRealtime(animationTimeInOut.x);

        while (true)
        {
            AnimateColor(uIColors.SelectedLighter, colorLoopTime, easing);
            yield return new WaitForSecondsRealtime(colorLoopTime);

            AnimateColor(uIColors.Selected, colorLoopTime, easing);
            yield return new WaitForSecondsRealtime(colorLoopTime);
        }
    }

    void StopSelectedColorLoop()
    {
        if (colorLoopCoroutine != null)
        {
            StopCoroutine(colorLoopCoroutine);
            colorLoopCoroutine = null;
        }
    }

    void OnValidate()
    {
        if (GameManager.GetInstance() is GameManager gameManager)
            GetComponent<Image>().color = gameManager.StaticConfig.UIColors.Default;
    }

    void OnCurrentHoldingButtonChange(ButtonTarget target)
    {
        // Animate pressed state
        if (target == this)
        {
            AnimateScale(
                scaleMinMax.x,
                animationTimeInOut.x * scaleTimeFactor,
                Ease.OutSine
            );
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        NavigationSystem.OnCurrentHoldingButtonChange += OnCurrentHoldingButtonChange;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        NavigationSystem.OnCurrentHoldingButtonChange -= OnCurrentHoldingButtonChange;
    }
}
