using DG.Tweening;
using UnityEngine;

/// <summary>Controls an indicator that shows the direction of the sled when it's off-screen.</summary>
/// <remarks>
/// Relies on <c>LevelManager.IsRunning</c> to be shown, so won't work in editor
/// unless a level starting sequence was played.
/// </remarks>
public class SledIndicator : MonoBehaviour
{
    [SerializeField] float offScreenDetectionMargin = 35f;
    [SerializeField] float onScreenShowPadding = 10f;

    [Header("Animation")]
    [SerializeField] float animationTime = 0.25f;
    [SerializeField] Ease animationEase = Ease.OutCubic;

    [Header("Indicators")]
    [SerializeField] GameObject indicatorLeft;
    [SerializeField] GameObject indicatorRight;

    SledController sledController;
    Vector2 screenPos;
    CanvasGroup canvasGroup;
    RectTransform rectTransform;
    [SerializeField] Canvas canvas;

    bool isShowing = false;
    bool isOnLeft = false;

    void Start()
    {
        sledController = Utils.GetComp<SledController>("Sled");
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        screenPos = Camera.main.WorldToScreenPoint(sledController.transform.position);
        bool isInsideTheScreen = screenPos.x >= -offScreenDetectionMargin && screenPos.x <= Screen.width + offScreenDetectionMargin;
        isOnLeft = screenPos.x < Screen.width / 2f;

        if (isShowing)
        {
            // If the sled is already showing, just update the position
            RefreshPosition();

            if (!LevelManager.I.IsRunning || isInsideTheScreen)
            {
                // Hide if its inside the screen or the level is not running
                Hide();
                return;
            }
        }
        else if (LevelManager.I.IsRunning && !isInsideTheScreen)
        {
            // If sled is off-screen, show indicator based on its position
            Show();
            return;
        }
    }

    void Show()
    {
        indicatorLeft.SetActive(isOnLeft);
        indicatorRight.SetActive(!isOnLeft);

        isShowing = true;
        RefreshVisual();
    }

    void Hide()
    {
        isShowing = false;
        RefreshVisual();
    }

    void RefreshVisual()
    {
        float target = isShowing ? 1 : 0f;

        canvasGroup.DOKill();
        rectTransform.DOKill();

        rectTransform
            .DOScale(target, animationTime)
            .SetEase(animationEase);

        canvasGroup
            .DOFade(target, animationTime)
            .SetEase(animationEase);
    }

    void RefreshPosition()
    {
        Vector2 clampedScreenPos = new(
            x: Mathf.Clamp(screenPos.x, onScreenShowPadding, Screen.width - onScreenShowPadding),
            y: Mathf.Clamp(screenPos.y, onScreenShowPadding, Screen.height - onScreenShowPadding)
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            clampedScreenPos,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        rectTransform.localPosition = localPoint;
    }
}
