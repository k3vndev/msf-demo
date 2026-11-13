using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ControlsTooltip : MonoBehaviour
{
    [SerializeField] string text;
    [Space]
    [SerializeField] float paddingX = 30;
    [SerializeField] float sledOffsetY = 40;

    [Header("Animation")]
    [SerializeField] Vector2 timeShowHide;
    [SerializeField] Ease easingShow;
    [SerializeField] Ease easingHide;

    [Header("Images")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Image shadowImage;

    public bool IsShowing { get; private set; } = false;

    bool isVisible = false;

    PlayerController playerController;
    SledController sledController;
    RectTransform rectTransform;
    [SerializeField] Canvas canvas;

    public TooltipName CurrentTooltip { get; private set; }
    TranslatedText translatedText;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        translatedText = GetComponent<TranslatedText>();

        translatedText.ParamGetter = () =>
        {
            string spriteName = ExtractSpriteName(CurrentTooltip);
            return $"<size=104%><sprite name=\"{spriteName}\"></size>";
        };
    }

    void Start()
    {
        playerController = Utils.GetComp<PlayerController>("Player");
        sledController = Utils.GetComp<SledController>("Sled");
    }

    void Update()
    {
        // Handle show/hide logic
        if (playerController.SledIsOnRange && !sledController.IsBeingCarried && !IsShowing
            && CurrentTooltip != GameManager.I.Global.LastUsedTooltip
            && playerController.CanInteract && !sledController.IsAdjustingPosition
            && playerController.CurrentSize == PlayerController.Size.Big)
        {
            Show();
        }
        else if (!playerController.SledIsOnRange && IsShowing)
        {
            Hide();
        }

        // Refresh position and tooltip
        if (isVisible)
            RefreshPosition();

        if (playerController.CanInteract && InputManager.I.IsUsingGameplay)
            CurrentTooltip = GetTooltipName();
    }

    void RefreshPosition()
    {
        Vector3 worldPos = sledController.transform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        localPoint.y += sledOffsetY;
        transform.localPosition = localPoint;
    }

    void Show()
    {
        IsShowing = true;
        AnimateScale();
        SetGlobalTooltip(TooltipName.None);
        RefreshLabel();
    }

    void Hide()
    {
        IsShowing = false;
        AnimateScale();
    }

    void AnimateScale()
    {
        var (easing, target, time) = IsShowing
            ? (easingShow, 1f, timeShowHide.x)
            : (easingHide, 0f, timeShowHide.y);

        if (IsShowing) isVisible = true;

        rectTransform.DOKill();
        rectTransform
            .DOScaleY(target, time)
            .SetEase(easing)
            .OnComplete(() =>
            {
                if (!IsShowing) isVisible = false;
            });
    }

    IEnumerator RefreshLabelCoroutine()
    {
        yield return null;
        translatedText.Refresh();
    }

    void RefreshLabel() => StartCoroutine(RefreshLabelCoroutine());

    TooltipName GetTooltipName()
    {
        if (InputManager.I.Detector.CurrentDevice == InputDeviceDetector.Device.Gamepad)
            return TooltipName.GamepadButtonSouth;

        if (InputManager.I.KeyboardSchemeSwaper.CurrentScheme == KeyboardSchemer.Scheme.WASD)
            return TooltipName.MouseClickLeft;

        return TooltipName.KeyboardKeyZ;
    }

    string ExtractSpriteName(TooltipName tooltip)
    {
        string tooltipStr = tooltip.ToString();
        string spriteName = "";

        for (int i = 0; i < tooltipStr.Length; i++)
        {
            string c = tooltipStr[i].ToString();

            if (c == c.ToUpper() && i != 0)
                spriteName += "-";

            spriteName += c.ToLower();
        }
        return spriteName;
    }

    void RefreshSize()
    {
        float width = translatedText.GetPreferredWidth() + paddingX * 2;
        float height = backgroundImage.rectTransform.sizeDelta.y;

        Image[] images = { backgroundImage, shadowImage };

        foreach (Image image in images)
            image.rectTransform.sizeDelta = new(width, height);
    }

    void OnSledCarryStart()
    {
        Hide();
        SetGlobalTooltip(CurrentTooltip);
    }

    void OnInputDetectorChangeDevice(InputDeviceDetector.Device device)
        => RefreshLabel();

    void OnKeyboardSchemerSwap(KeyboardSchemer.Scheme scheme)
        => RefreshLabel();

    void SetGlobalTooltip(TooltipName tooltip)
        => GameManager.I.Global.LastUsedTooltip = tooltip;

    void OnEnable()
    {
        KeyboardSchemer.OnSwap += OnKeyboardSchemerSwap;
        InputDeviceDetector.OnChangeDevice += OnInputDetectorChangeDevice;
        SledController.OnCarryStart += OnSledCarryStart;
        translatedText.OnRefresh += RefreshSize;
    }

    void OnDisable()
    {
        KeyboardSchemer.OnSwap -= OnKeyboardSchemerSwap;
        InputDeviceDetector.OnChangeDevice -= OnInputDetectorChangeDevice;
        SledController.OnCarryStart -= OnSledCarryStart;
        translatedText.OnRefresh -= RefreshSize;
    }

    public enum TooltipName
    {
        None,
        MouseClickLeft,
        GamepadButtonSouth,
        KeyboardKeyZ
    }
}

