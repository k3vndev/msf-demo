using UnityEngine;

/// <summary>
/// Locks camera framing to the virtual resolution defined by ScreenSizing.
/// Prevents aspect ratio changes from affecting gameplay visibility.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFramer : MonoBehaviour
{
    Camera _camera;

    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void Start()
    {
        RefreshCameraAspect();
    }

    void RefreshCameraAspect()
    {
        float targetAspectRatio = ScreenSizing.I.VirtualResolution.x / ScreenSizing.I.VirtualResolution.y;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspectRatio;

        if (scaleHeight < 1.0f)
        {
            // Letterboxing
            Rect rect = _camera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            _camera.rect = rect;
        }
        else
        {
            // Pillarboxing
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = _camera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            _camera.rect = rect;
        }
    }

    void OnEnable()
    {
        ScreenSizing.OnVirtualResolutionChanged += RefreshCameraAspect;
    }

    void OnDisable()
    {
        ScreenSizing.OnVirtualResolutionChanged -= RefreshCameraAspect;
    }
}