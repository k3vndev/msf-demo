using UnityEngine;
using UnityEngine.UI;

public class SwitchButton : LighteableImage, IPointerPressable
{
    [SerializeField] int direction;
    SwitchTarget parentSwitch;

    protected override void Awake()
    {
        base.Awake();
        parentSwitch = GetComponentInParent<SwitchTarget>();
    }

    void OnParentSwitchStateChanged(bool isSelected)
    {
        if (isSelected)
        {
            BlendHigh();
            return;
        }
        BlendLow();
    }

    void OnEnable() => parentSwitch.OnVisualsChanged += OnParentSwitchStateChanged;
    void OnDisable() => parentSwitch.OnVisualsChanged -= OnParentSwitchStateChanged;

    public void PointerPress()
    {
        parentSwitch.ChangeValue(direction, 1f);
    }

    void OnValidate()
    {
        // Set the raycast padding to ensure proper interaction
        Image image = GetComponent<Image>();
        Vector2 padding = new(-6f, -11f);
        image.raycastPadding = new Vector4(padding.x, padding.y, padding.x, padding.y);
    }

    public void PointerRelease() { }
}