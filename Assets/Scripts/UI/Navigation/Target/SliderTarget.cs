using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderTarget : ChangeableNavigationTarget
{
    [Header("Slider")]
    [SerializeField] string label;
    [SerializeField] UnityEvent<float> onValueChanged;

    [Header("TM PRO")]
    [SerializeField] TextMeshProUGUI labelTmp;
    [SerializeField] TextMeshProUGUI valueTmp;

    [Header("Images")]
    [SerializeField] LighteableImage handlerLighteableImage;
    [SerializeField] Image backgroundImage;
    [SerializeField] Image fillImage;

    [SerializeField] Slider slider;

    protected override void Start()
    {
        base.Start();
        RefreshValueDisplay();
    }

    public override void ChangeValue(int direction, float looseValue)
    {
        base.ChangeValue(direction, looseValue);
        int addition = Mathf.RoundToInt(direction * Mathf.Min(1f, looseValue));
        SetValue(slider.value + addition);
    }

    public void SetValue(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0, 100);
        slider.value = clampedValue;

        onValueChanged?.Invoke(clampedValue);
        RefreshValueDisplay();
    }

    public override void RefreshValueDisplay()
    {
        base.RefreshValueDisplay();
        valueTmp.text = ((int)slider.value).ToString();
    }

    public override void EnableSelectedVisuals()
    {
        base.EnableSelectedVisuals();
        handlerLighteableImage.BlendFull();
    }

    public override void DisableSelectedVisuals()
    {
        base.DisableSelectedVisuals();
        handlerLighteableImage.BlendHigh();
    }

    protected override void OnChangeCanNavigate(bool canNavigate)
    {
        base.OnChangeCanNavigate(canNavigate);

        if (CanvasGroup)
        {
            CanvasGroup.interactable = canNavigate;
            CanvasGroup.blocksRaycasts = canNavigate;
        }
    }

    void OnValidate()
    {
        // Ensure the label is set
        labelTmp.text = label;

        // Ensure the value text is initialized
        if (GameManager.GetInstance() is GameManager gameManager)
        {
            backgroundImage.color = gameManager.StaticConfig.UIColors.Default;
            fillImage.color = gameManager.StaticConfig.UIColors.DefaultLighter;
        }

        // Set raycast padding for images to ensure proper interaction
        Image handlerImage = handlerLighteableImage.GetComponent<Image>();
        Image[] images = { backgroundImage, handlerImage, fillImage };
        foreach (Image image in images)
        {
            float padding = -4f;
            image.raycastPadding = Vector4.one * padding;
        }
    }
}