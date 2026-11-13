using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ElementAnimator : MonoBehaviour
{
    [SerializeField] Vector2 slideAmount;

    [Header("Animation Time")]
    [SerializeField] float animationTimeHide = 0.5f;
    [SerializeField] float animationTimeShow = 0.85f;

    [Header("Move Easing")]
    [SerializeField] Ease easingHideMove = Ease.InQuad;
    [SerializeField] Ease easingShowMove = Ease.OutBack;

    [Header("Alpha Easing")]
    [SerializeField] Ease easingHideAlpha = Ease.InQuart;
    [SerializeField] Ease easingShowAlpha = Ease.OutQuart;

    RectTransform rect;
    Vector3 visiblePosition;
    Vector3 hiddenPosition;
    CanvasGroup canvasGroup;

    public bool IsSliding { get; private set; } = false;
    [field: SerializeField] public bool IsHidden { get; private set; } = false;

    void OnEnable()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        visiblePosition = rect.anchoredPosition;
        hiddenPosition = visiblePosition + (Vector3)slideAmount;

        Slide(IsHidden, true);
    }

    public void Hide() => Slide(isHiding: true);
    public void HideInstant() => Slide(isHiding: true, instant: true);

    public void Show() => Slide(isHiding: false);
    public void ShowInstant() => Slide(isHiding: false, instant: true);

    void Slide(bool isHiding, bool instant = false)
    {
        // Declare neede variables
        var (moveTarget, alphaTarget, time, moveEasing, alphaEasing) = isHiding
            ? (hiddenPosition, 0f, animationTimeHide, easingHideMove, easingHideAlpha)
            : (visiblePosition, 1f, animationTimeShow, easingShowMove, easingShowAlpha);

        if (instant)
        {
            // Execute instantly and exit
            rect.anchoredPosition = moveTarget;
            canvasGroup.alpha = alphaTarget;
            SetCanvasInteractable(!isHiding);
            return;
        }

        IsSliding = true;
        SetCanvasInteractable(true);

        // Move to target
        rect.DOKill();
        rect
            .DOAnchorPos(moveTarget, time)
            .SetEase(moveEasing)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(() =>
            {
                IsSliding = false;
                IsHidden = isHiding;
            });

        // Fade to target
        canvasGroup.DOKill();
        canvasGroup.DOFade(alphaTarget, time)
            .SetEase(alphaEasing)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(() => SetCanvasInteractable(!isHiding));
    }

    void SetCanvasInteractable(bool value)
    {
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }

    void OnValidate()
    {
        // Keep consistency between IsHidden and canvasGroup values
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = IsHidden ? 0 : 1;
        canvasGroup.interactable = !IsHidden;
        canvasGroup.blocksRaycasts = !IsHidden;
    }
}
