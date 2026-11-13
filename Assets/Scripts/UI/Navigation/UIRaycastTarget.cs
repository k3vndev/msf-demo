using UnityEngine;
using UnityEngine.UI;

public class UIRaycastTarget : Image
{
    public readonly float xPadding = -20;

    protected override void Awake()
    {
        base.Awake();
        color = new Color(0, 0, 0, 0);
    }

    void OnValidate()
    {
        raycastTarget = true;

        raycastPadding = new Vector4(
            x: xPadding,
            y: -10,
            z: xPadding,
            w: -5
        );
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return true;
    }
}