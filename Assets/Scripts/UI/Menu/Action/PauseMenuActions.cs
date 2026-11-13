
using System;

public class PauseMenuActions : MenuActions
{
    public static Action OnRestartLevel;
    public static Action OnQuitLevel;

    public void ResumeButton()
        => PauseMenuManager.I.Resume();

    public void RestartButton()
    {
        OnRestartLevel?.Invoke();
        float transitionTime = 0.9f;

        NavigationSystem.I.DisableNavigation();

        TransitionManager.I
            .Towards(SceneTarget.CurrentLevel)
            .SetTransition(Transition.WhiteFade)
            .SetDuration(transitionTime)
            .OnEarlyNewSceneLoaded(() =>
            {
                ResetTime();
                StartLevel();
            })
            .OnTransitionOutComplete(() => PauseMenuManager.I.EnablePausing())
            .SkipLoadingScreen();
    }

    public void SettingsButton()
        => PauseMenuManager.I.FromPauseToSettings();

    public void MainMenuButton()
    {
        OnQuitLevel?.Invoke();
        float transitionTime = 1.1f;
        float enableNavTime = 0.9f;

        NavigationSystem.I.DisableNavigation();

        TransitionManager.I
            .Towards(Scenes.UI.MainMenu.Index)
            .SetTransition(Transition.Circle)
            .SetDuration(transitionTime)
            .OnEarlyNewSceneLoaded(() =>
            {
                ResetTime();

                Utils.Time.After(enableNavTime, () =>
                    NavigationSystem.I.EnableNavigation());
            });
    }
}