using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Responsible of screen size and resolution behavior.</summary>
public class ScreenSizing : MonoBehaviour
{
    public static ScreenSizing I { get; private set; }

    /// <summary>Fired every time the screen resizes.</summary>
    public static event Action OnScreenResized;

    public const float INIT_WINDOWED_FACTOR = 0.88f;
    public const int MIN_WIDTH = 640;
    public const int MIN_HEIGTH = 360;
    int lastWidth;
    int lastHeight;

    GlobalState global;

    public const float MinAspectRatio = 1.6f;
    public const float MaxAspectRatio = 2.34f;

    public Vector2 VirtualResolution { get; private set; }
    public float CurrentAspect { get; private set; }
    public static event Action OnVirtualResolutionChanged;

    Vector2Int ValidResolution
    {
        get => new(Mathf.Max(MIN_WIDTH, Screen.width),
             Mathf.Max(MIN_HEIGTH, Screen.height));
    }

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    void Start()
    {
        global = GameManager.I.Global;

        // Set the initial resolution
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        HandleScreenResize();
    }

    void Update()
    {
        if (lastWidth != Screen.width || lastHeight != Screen.height)
        {
            // Manually listen for resize events
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            HandleScreenResize();
        }
    }

    /// <summary>Sets the fullscreen mode and updates the global state.</summary>
    public void SetFullscreen(bool settingFullscreen)
    {
        if (settingFullscreen)
        {
            TrySaveWindowedSize();

            // Set the full resolution and enable fullscreen
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        }
        else
        {
            // Get the stored windowed resolution if valid or calc a new one
            var (width, height) = global.WindowedWidth >= MIN_WIDTH && global.WindowedHeight >= MIN_HEIGTH
                ? (global.WindowedWidth, global.WindowedHeight)
                : ((int)(Screen.width * INIT_WINDOWED_FACTOR), (int)(Screen.height * INIT_WINDOWED_FACTOR));

            // Apply resolution
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
        }

        global.Settings.Fullscreen = settingFullscreen;
    }

    void HandleScreenResize()
    {
        OnScreenResized?.Invoke();
        TrySaveWindowedSize();

        // Handle aspect ratio and virtual resolution
        CurrentAspect = (float)Screen.width / Screen.height;

        float targetAspect = Mathf.Clamp(CurrentAspect, MinAspectRatio, MaxAspectRatio);

        int width = Screen.width;
        int height = Screen.height;

        // Build a consistent virtual resolution
        if (CurrentAspect > MaxAspectRatio)
            width = Mathf.RoundToInt(height * MaxAspectRatio);

        else if (CurrentAspect < MinAspectRatio)
            height = Mathf.RoundToInt(width / MinAspectRatio);

        // Set global variable and invoke event
        VirtualResolution = new Vector2(width, height);
        OnVirtualResolutionChanged?.Invoke();
    }

    /// <summary>Tries to save the current windowed resolution to the global state.</summary>
    void TrySaveWindowedSize()
    {
        if (Screen.fullScreen) return;

        // Save previous resolution
        global.WindowedWidth = ValidResolution.x;
        global.WindowedHeight = ValidResolution.y;
        SaveSystem.I.RequestSave();
    }

    void OnToggleFullscreenPerformed(InputAction.CallbackContext _)
    {
        SetFullscreen(!global.Settings.Fullscreen);

        SettingsMenuActions settingsMenu = FindFirstObjectByType<SettingsMenuActions>();
        if (settingsMenu == null) return;

        settingsMenu.RefreshValues();
    }

    void OnEnable()
    {
        InputManager.I.Controls.Global.ToggleFullscreen.performed += OnToggleFullscreenPerformed;
    }

    void OnDisable()
    {
        InputManager.I.Controls.Global.ToggleFullscreen.performed -= OnToggleFullscreenPerformed;
    }
}
