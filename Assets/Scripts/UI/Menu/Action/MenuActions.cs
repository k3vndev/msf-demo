using UnityEngine;

public abstract class MenuActions : MonoBehaviour
{
    protected void StartLevel()
    {
        if (LevelSequencer.I?.Characters != null)
        {
            LevelSequencer.I.Characters.InitializePosition();
            LevelSequencer.I.Characters.StartEntranceSequence();
        }
        else
        {
            Debug.LogWarning($"MenuActions: LevelsSequencer but wasn't initialized yet at ${name}!");
        }

        NavigationSystem.I.EnableNavigation();
        InputManager.UseGameplay();
        PauseMenuManager.I.DisablePausing();
    }

    protected void ResetTime() => Time.timeScale = 1;
}