using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionRequest
{
    readonly int sceneIndex;
    Transition transitionIn = Transition.None;
    Transition transitionOut = Transition.None;
    float durationIn = -1;
    float durationOut = -1;
    bool skipLoadingScreen = false;

    Action onEarlyTargetSceneLoaded;
    Action onTargetSceneLoaded;
    Action onTransitionOutComplete;
    Action onTransitionInComplete;

    public TransitionRequest(int sceneIndex)
        => this.sceneIndex = sceneIndex;

    ///<summary>Sets both the animations for the transition.</summary>
    public TransitionRequest SetTransition(Transition transition)
        => SetTransition(transition, transition);

    ///<summary>Sets the animations for the transition individually.</summary>
    public TransitionRequest SetTransition(Transition @in, Transition @out)
    {
        transitionIn = @in;
        transitionOut = @out;
        return this;
    }

    ///<summary>
    /// Sets both the animation durations for the transition.
    /// Negative values are replaced with default duration.
    /// </summary>
    public TransitionRequest SetDuration(float duration)
        => SetDuration(duration, duration);

    ///<summary>
    /// Sets the animation durations for the transition individually.
    /// Negative values are replaced with default duration.
    /// </summary>
    public TransitionRequest SetDuration(float @in, float @out)
    {
        durationIn = @in;
        durationOut = @out;
        return this;
    }

    ///<summary>Called one frame after the target scene loads.</summary>
    public TransitionRequest OnNewSceneLoaded(Action callback)
    {
        onTargetSceneLoaded = callback;
        return this;
    }

    ///<summary>Called as soon as the target scene loads and before the Start methods are called.</summary>
    public TransitionRequest OnEarlyNewSceneLoaded(Action callback)
    {
        onEarlyTargetSceneLoaded = callback;
        return this;
    }

    ///<summary>Called as soon as the in transition completes and before unloading the starting scene.</summary>
    public TransitionRequest OnTransitionInComplete(Action callback)
    {
        onTransitionInComplete = callback;
        return this;
    }

    ///<summary>Called as soon as the out transition completes on the target scene.</summary>
    public TransitionRequest OnTransitionOutComplete(Action callback)
    {
        onTransitionOutComplete = callback;
        return this;
    }

    ///<summary>Skips the loading screen by loading the target scene directly.</summary>
    public TransitionRequest SkipLoadingScreen()
    {
        skipLoadingScreen = true;
        return this;
    }

    /// <summary>TransitionManager's internal method to execute the transition request. Never call this method directly.</summary>
    /// <remarks>Transition starts one frame after being called.</remarks>
    public IEnumerator Run()
    {
        // Wait a frame for configuration methods to be called and the transition request to be fully set up
        yield return null;

        ActivateTransitioners(transitionIn);

        // Load "Loading Screen" scene or skip and go directly to the target scene
        if (skipLoadingScreen) yield return WithoutLoadingScreen();
        else yield return WithLoadingScreen();

        // Invoke scene load callback
        onEarlyTargetSceneLoaded?.Invoke();

        yield return null;

        // Invoke target scene load callback
        onTargetSceneLoaded?.Invoke();

        // Stop loading screen if it was shown
        LoadingScreen.I.StopLoading();

        // Perform transition "out" and wait until it ends
        ActivateTransitioners(transitionOut);

        if (TransitionManager.I.RetrieveTransitioner(transitionOut) is Transitioner transitionerOut)
        {
            transitionerOut.TransitionOut(durationOut);
            yield return new WaitUntil(() => !transitionerOut.IsTransitioning);
        }

        // Invoke transition out callback
        onTransitionOutComplete?.Invoke();
    }

    IEnumerator WithLoadingScreen()
    {
        // Start loading Loading Screen scene
        AsyncOperation loadingScreen_op = StartSceneOperation(Scenes.UI.LoadingScreen.Index);

        // Perform transition in and wait until it ends
        yield return PerformTransitionIn();

        // Wait for "Loading Screen" scene to finish loading and complete
        yield return CompleteSceneOperation(loadingScreen_op);

        // Start loading target scene and wait until it completes
        AsyncOperation targetScOperation = StartSceneOperation(sceneIndex);
        yield return CompleteSceneOperation(targetScOperation);
    }

    IEnumerator WithoutLoadingScreen()
    {
        // Load target scene
        AsyncOperation targetScOperation = StartSceneOperation(sceneIndex);

        // Perform transition in and wait until it ends
        yield return PerformTransitionIn();

        // Wait for target scene to finish loading and complete
        yield return CompleteSceneOperation(targetScOperation);
    }

    IEnumerator PerformTransitionIn()
    {
        if (TransitionManager.I.RetrieveTransitioner(transitionIn) is Transitioner transitionerIn)
        {
            float realDurationIn = transitionerIn.TransitionIn(durationIn);

            // Only show the loading screen if it wasn't skipped
            yield return HandleAppearLoadingScreen(realDurationIn);

            // Wait for the transition in to complete and invoke the callback
            yield return new WaitUntil(() => !transitionerIn.IsTransitioning);
            onTransitionInComplete?.Invoke();
        }
    }

    ///<summary>Waits for the transition to be almost complete and then starts the loading screen.</summary>
    IEnumerator HandleAppearLoadingScreen(float realDurationIn)
    {
        if (!skipLoadingScreen)
        {
            // Wait for the transition to be almost complete and then start the loading screen
            float elapsed = 0f;
            while (elapsed < realDurationIn - LoadingScreen.I.FadeDurationInOut.x)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            LoadingScreen.I.StartLoading();
        }
    }

    /// <summary>
    /// Activates the transitioners for the transition request, taking into account dependencies and exceptions.<br/>
    /// Also deactivates transitioners that are not needed for this transition.
    /// </summary>
    void ActivateTransitioners(Transition transition)
    {
        // Skip if there's no transition
        if (transition == Transition.None) return;

        // Get transitioner for the transition and add it to the list of transitioners to not hide
        HashSet<Transitioner> transitionersToNotHide = new();
        Transitioner transitioner = TransitionManager.I.RetrieveTransitioner(transition);
        transitionersToNotHide.Add(transitioner);

        // If the transitioner has exceptions, add them to the list of transitioners to not hide as well
        if (transitioner.TransitionDependencies.Length > 0)
        {
            Transitioner[] exceptions = transitioner.TransitionDependencies
                .Select(t => TransitionManager.I.RetrieveTransitioner(t))
                .ToArray();

            transitionersToNotHide.UnionWith(exceptions);
        }

        // Enable/Disable transitioners based on whether they are in the list of transitioners to not hide
        foreach (Transitioner t in TransitionManager.I.Transitioners)
        {
            bool shouldBeActive = transitionersToNotHide.Contains(t);
            if (!shouldBeActive && t?.gameObject.activeInHierarchy == true) t.Reset();
            t.gameObject.SetActive(shouldBeActive);
        }
    }

    AsyncOperation StartSceneOperation(int buildIndex)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
        op.allowSceneActivation = false;
        return op;
    }

    IEnumerator CompleteSceneOperation(AsyncOperation op)
    {
        yield return new WaitUntil(() => op.progress >= 0.9f);
        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);
    }
}
