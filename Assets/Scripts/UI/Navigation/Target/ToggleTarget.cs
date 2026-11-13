using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleTarget : ChangeableNavigationTarget
{
    [Space]
    [SerializeField] UnityEvent<bool> onValueChanged;
    [SerializeField] Image backgroundImage;

    [Header("Check Mark")]
    [SerializeField] LighteableImage checkMarkLigtheableImage;
    [SerializeField] Vector2 checkAnimationTimeInOut;

    public bool IsChecked { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake();
        RefreshCheckMark();
    }

    public override void Accept()
    {
        base.Accept();
        SetValue(!IsChecked);
    }

    public void SetValue(bool value)
    {
        if (IsChecked == value) return;

        IsChecked = value;
        onValueChanged?.Invoke(IsChecked);
        RefreshCheckMark();
    }

    public override void EnableSelectedVisuals()
    {
        base.EnableSelectedVisuals();
        RefreshCheckMark();
    }

    public override void DisableSelectedVisuals()
    {
        base.DisableSelectedVisuals();
        RefreshCheckMark();
    }

    void RefreshCheckMark()
    {
        if (IsChecked && IsSelected)
            checkMarkLigtheableImage.BlendFull(checkAnimationTimeInOut.x);

        else if (IsChecked && !IsSelected)
            checkMarkLigtheableImage.BlendHigh(checkAnimationTimeInOut.y);

        else if (!IsChecked)
            checkMarkLigtheableImage.BlendNone(checkAnimationTimeInOut.y);
    }

    public override void ChangeValue(int direction, float _)
    {
        base.ChangeValue(direction, _);
        Accept();
    }

    void OnValidate()
    {
        if (GameManager.GetInstance() is GameManager gameManager)
            backgroundImage.color = gameManager.StaticConfig.UIColors.Default;
    }
}