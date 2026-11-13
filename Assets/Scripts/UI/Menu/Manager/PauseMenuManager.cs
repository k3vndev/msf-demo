using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MenuManager
{
    public static PauseMenuManager I { get; private set; }
    public static event Action<bool> OnTogglePause;

    [Header("Element Animators")]
    [SerializeField] ElementAnimator backgroundAnimator;
    [SerializeField] ElementAnimator pauseMenuAnimator;
    [SerializeField] ElementAnimator settingsMenuAnimator;

    [Header("Navigation Targets")]
    [SerializeField] NavigationTarget pause_ResumeButton;
    [SerializeField] NavigationTarget pause_SettingsButton;
    [SerializeField] NavigationTarget settings_OkayButton;

    public bool IsPaused { get; private set; } = false;
    public bool CanPause { get; private set; } = true;

#if !UNITY_EDITOR 
    Coroutine applicationUnFocusCoroutine;
#endif

    void Awake() => I = this;
    void OnDestroy() => I = null;

    void Start()
    {
        NavigationSystem.I.Initialize(pauseMenuAnimator.gameObject);
        NavigationSystem.I.Select(pause_ResumeButton);
    }

    public void Pause()
    {
        if (IsPaused || !CanPause || TransitionManager.I.IsTransitioning) return;

        if (NavigationSystem.I.IsUsingPointer)
        {
            NavigationSystem.I.StopNavigating();
            GameManager.SetCursorVisibility(true);
        }

        NavigationSystem.I.Select(pause_ResumeButton);
        InputManager.UseMenu();
        backgroundAnimator.Show();

        Time.timeScale = 0;
        OnTogglePause?.Invoke(true);
        IsPaused = true;
    }

    public void Resume()
    {
        if (!IsPaused) return;
        StartCoroutine(ResumeSequence());
    }

    IEnumerator ResumeSequence()
    {
        Time.timeScale = 1;
        OnTogglePause?.Invoke(false);
        IsPaused = false;

        InputManager.UseGameplay();
        backgroundAnimator.Hide();
        GameManager.SetCursorVisibility(false);

        yield return new WaitUntil(() => !backgroundAnimator.IsSliding);
        NavigationSystem.I.Select(pause_ResumeButton);
    }

    public void FromPauseToSettings()
    {
        StartCoroutine(SwapMenu(
            hideElement: pauseMenuAnimator,
            showElement: settingsMenuAnimator,
            selectTarget: settings_OkayButton
        ));
        IsOnSettingsMenu = true;
    }

    public void FromSettingsToPause()
    {
        StartCoroutine(SwapMenu(
            hideElement: settingsMenuAnimator,
            showElement: pauseMenuAnimator,
            selectTarget: pause_SettingsButton
        ));
        IsOnSettingsMenu = false;
    }

    void OnPauseButtonPerformed(InputAction.CallbackContext _) => Pause();

    protected override void OnNavigationCancel()
    {
        if (IsOnSettingsMenu)
        {
            FromSettingsToPause();
            return;
        }
        Resume();
    }

    public void EnablePausing() => CanPause = true;
    public void DisablePausing() => CanPause = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        InputManager.Gameplay.Pause.performed += OnPauseButtonPerformed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InputManager.Gameplay.Pause.performed -= OnPauseButtonPerformed;
    }

#if !UNITY_EDITOR
    // Handle automatic pause on unfocus

    void OnApplicationFocus(bool hasFocus)
    {
        HandleApplicationUnFocus(!hasFocus);
    }

    void OnApplicationPause(bool isPaused)
    {
        HandleApplicationUnFocus(isPaused);
    }

    void HandleApplicationUnFocus(bool unFocused)
    {
        if (unFocused)
        {
            applicationUnFocusCoroutine = StartCoroutine(ApplicationUnFocus());
            return;
        }

        if (applicationUnFocusCoroutine != null)
        {
            StopCoroutine(applicationUnFocusCoroutine);
            applicationUnFocusCoroutine = null;
        }
    }

    IEnumerator ApplicationUnFocus()
    {
        if (IsPaused) yield break;
        yield return new WaitUntil(() => CanPause && !TransitionManager.I.IsTransitioning);

        Pause();
    }
#endif
}