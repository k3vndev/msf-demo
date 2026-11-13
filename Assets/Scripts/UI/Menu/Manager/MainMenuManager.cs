using System;
using UnityEngine;

public class MainMenuManager : MenuManager
{
    public static MainMenuManager I { get; private set; }

    public static event Action OnSwap;

    [Header("Buttons")]
    [SerializeField] NavigationTarget settings_OkayButton;
    [SerializeField] NavigationTarget main_SettingsButton;
    [SerializeField] NavigationTarget main_PlayButton;

    [Header("Element Animations")]
    [SerializeField] ElementAnimator titleAnimator;
    [SerializeField] ElementAnimator mainMenuAnimator;
    [SerializeField] ElementAnimator settingsMenuAnimator;

    void Awake()
    {
        I = this;
        InputManager.UseMenu();
        GameManager.SetCursorVisibility(true);
    }

    void Start()
    {
        NavigationSystem.I.Initialize(mainMenuAnimator.gameObject);
        NavigationSystem.I.Select(main_PlayButton);
        MusicManager.I.PlayAmbience();
    }

    public void FromMainToSettings()
    {
        IsOnSettingsMenu = true;
        titleAnimator.Hide();
        OnSwap?.Invoke();

        StartCoroutine(SwapMenu(
            showElement: settingsMenuAnimator,
            hideElement: mainMenuAnimator,
            selectTarget: settings_OkayButton
        ));
    }

    public void FromSettingsToMain()
    {
        IsOnSettingsMenu = false;
        titleAnimator.Show();
        OnSwap?.Invoke();

        StartCoroutine(SwapMenu(
            showElement: mainMenuAnimator,
            hideElement: settingsMenuAnimator,
            selectTarget: main_SettingsButton
        ));
    }

    protected override void OnNavigationCancel()
    {
        base.OnNavigationCancel();
        if (IsOnSettingsMenu) FromSettingsToMain();
    }
}
