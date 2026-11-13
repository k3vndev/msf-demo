using UnityEngine.UI;
using UnityEngine;

public class SwitchLine : LighteableImage, IPointerPressable
{
    SwitchTarget parentSwitch;
    int index;

    protected override void Awake()
    {
        base.Awake();
        parentSwitch = GetComponentInParent<SwitchTarget>();

        // Get the index of this SwitchLine
        for (int i = 0; i < parentSwitch.Options.Length; i++)
        {
            if (parentSwitch.Options[i].SwitchLine == this)
            {
                index = i;
                break;
            }
        }
    }

    public void Choose()
    {
        if (parentSwitch.IsSelected && NavigationSystem.I.IsNavigating)
        {
            BlendFull();
            return;
        }
        BlendHigh();
    }

    public void UnChoose() => BlendLow();

    public void PointerPress()
    {
        parentSwitch.SetValue(index);
    }

    void OnValidate()
    {
        // Set the raycast padding to ensure proper interaction
        Image image = GetComponent<Image>();
        Vector2 padding = new(-9f, -14f);
        image.raycastPadding = new Vector4(padding.x, padding.y, padding.x, padding.y);
    }

    public void PointerRelease() { }
}