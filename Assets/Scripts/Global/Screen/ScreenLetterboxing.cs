using UnityEngine;
using UnityEngine.UI;

public class ScreenLetterboxing : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] Image boxingImageA;
    [SerializeField] Image boxingImageB;

    void HandleScreenResize()
    {
        float lastWidth = Screen.width;
        float lastHeight = Screen.height;

        float currentAspectRatio = (float)lastWidth / lastHeight;

        if (currentAspectRatio < ScreenSizing.MinAspectRatio)
        {
            EnableImages(true);

            // Add letterboxing (horizontal bars)
            float expectedHeight = ScreenSizing.I.VirtualResolution.y;
            float size = (lastHeight - expectedHeight) / 2f;

            StretchImage(boxingImageA, Axis.Horizontal, Alignment.TopRight, size);
            StretchImage(boxingImageB, Axis.Horizontal, Alignment.BottomLeft, size);
        }
        else if (currentAspectRatio > ScreenSizing.MaxAspectRatio)
        {
            EnableImages(true);

            // Add pillarboxing (vertical bars)
            float expectedWidth = ScreenSizing.I.VirtualResolution.x;
            float size = (lastWidth - expectedWidth) / 2f;

            StretchImage(boxingImageA, Axis.Vertical, Alignment.TopRight, size);
            StretchImage(boxingImageB, Axis.Vertical, Alignment.BottomLeft, size);
        }
        else
        {
            // Screen is within bounds
            EnableImages(false);
        }
    }

    enum Alignment { BottomLeft, TopRight }
    void StretchImage(Image img, Axis axis, Alignment alignMode, float crossAxisSize)
    {
        RectTransform rt = img.rectTransform;
        if (!rt) return;

        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        int alignment = (int)alignMode;

        if (axis == Axis.Horizontal)
        {
            rt.anchorMin = new Vector2(0, alignment);
            rt.anchorMax = new Vector2(1, alignment);
            rt.pivot = new Vector2(0.5f, alignment);

            rt.sizeDelta = new Vector2(0, crossAxisSize);
        }
        else
        {
            rt.anchorMin = new Vector2(alignment, 0);
            rt.anchorMax = new Vector2(alignment, 1);
            rt.pivot = new Vector2(alignment, 0.5f);

            rt.sizeDelta = new Vector2(crossAxisSize, 0);
        }
    }

    void EnableImages(bool enable)
    {
        boxingImageA.gameObject.SetActive(enable);
        boxingImageB.gameObject.SetActive(enable);
    }

    void OnEnable()
    {
        ScreenSizing.OnVirtualResolutionChanged += HandleScreenResize;
    }

    void OnDisable()
    {
        ScreenSizing.OnVirtualResolutionChanged -= HandleScreenResize;
    }
}