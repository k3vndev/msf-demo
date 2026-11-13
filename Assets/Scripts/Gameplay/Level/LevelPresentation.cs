using System.Collections;
using UnityEngine;

public class LevelPresentation
{
    public const float EntranceDelay = 3.62f;
    public const float FadeTime = 2.5f;

    public IEnumerator Run()
    {
        float entranceTime = Time.time + EntranceDelay;

        WhiteFadeTransitioner transitioner = TransitionManager.I.WhiteFadeTransitioner;
        transitioner.gameObject.SetActive(true);

        PauseMenuManager.I?.DisablePausing();
        LevelSequencer.I?.Characters.InitializePosition();

        // Play camera and fade animations
        LevelCamerasManager.I?.DollyAnimation();
        transitioner.TransitionOut(FadeTime);

        // Wait for transition out to stop before enabling pausing again
        yield return new WaitUntil(() => !transitioner.IsTransitioning);
        PauseMenuManager.I?.EnablePausing();
        transitioner.gameObject.SetActive(false);

        // Wait for entrance time to start player and sled entrance sequence
        yield return new WaitUntil(() => Time.time >= entranceTime);
        LevelSequencer.I?.Characters.StartEntranceSequence();
    }
}
