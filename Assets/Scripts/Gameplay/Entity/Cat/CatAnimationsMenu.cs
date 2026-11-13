using UnityEngine;

public class CatAnimationsMenu : CatAnimations
{
    void OnMainMenuManagerSwap()
    {
        if (MainMenuManager.I?.IsOnSettingsMenu ?? false)
        {
            StartIdleProcess();
            return;
        }
        StopIdleProcess();
    }

    void OnEnable() => MainMenuManager.OnSwap += OnMainMenuManagerSwap;
    void OnDisable() => MainMenuManager.OnSwap -= OnMainMenuManagerSwap;
}