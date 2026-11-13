using UnityEngine;

public class MainMenuActions : MenuActions
{
    public void PlayButton()
    {
        NavigationSystem.I.DisableNavigation();
        MusicManager.I.StopAmbience();

        TransitionManager.I
            .Towards(SceneTarget.CurrentLevel)
            .SetTransition(Transition.WhiteFade, Transition.Circle)
            .SetDuration(1.2f, 1f)
            .OnEarlyNewSceneLoaded(StartLevel)
            .OnTransitionOutComplete(() => PauseMenuManager.I.EnablePausing());
    }

    public void SettingsButton()
        => MainMenuManager.I.FromMainToSettings();

    public void QuitButton()
    {
        Debug.LogWarning("Exiting game...");
        Application.Quit();
    }
}
