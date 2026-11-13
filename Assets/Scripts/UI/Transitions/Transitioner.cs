using System;
using UnityEngine;

/// <summary>Base class for all transitions.</summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class Transitioner : MonoBehaviour
{
    /// <summary>Default duration in seconds for this transition. Can be overridden when performing the transition.</summary>
    [field: Tooltip("Default duration in seconds for this transition. Can be overridden when performing the transition.")]
    [field: SerializeField] public float Duration { get; private set; }

    /// <summary>Whether a transition is currently in progress.</summary>
    public bool IsTransitioning { get; protected set; }

    public CanvasGroup CanvasGroup { get; private set; }

    /// <summary>Transitions that are required for this transition to work properly.</summary>
    [field: Tooltip("Transitions that are required for this transition to work properly.")]
    [field: SerializeField] public Transition[] TransitionDependencies { get; protected set; }


    /// <summary>
    /// Event invoked at the start of the transition, after setting the <c>IsTransitioning</c> flag to true and before performing the transition.
    /// </summary>
    public Action TransitionStarted;
    /// <summary>
    /// Event invoked at the end of the transition, after setting the <c>IsTransitioning</c> flag to false and after performing the transition.
    /// </summary>
    public Action TransitionFinished;


    protected virtual void Awake()
    {
        IsTransitioning = false;
        CanvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>Starts a transition using the default duration.</summary>
    /// <param name="backwards">If true, performs the transition in reverse.</param>
    public void Transitionate(bool backwards = false) => Transitionate(Duration, backwards);

    /// <summary>Starts a transition using a custom duration.</summary>
    /// <param name="time">The duration of the transition in seconds. Negative values are overriden by the default value.</param>
    /// <param name="backwards">If true, performs the transition in reverse.</param>
    /// <returns>The actual duration of the transition in seconds.</returns>
    public float Transitionate(float time, bool backwards = false)
    {
        float t = time > 0 ? time : Duration;
        return PerformTransition(t, backwards);
    }

    /// <summary>Starts an "in" transition using the default duration.</summary>
    /// <returns>The actual duration of the transition in seconds.</returns>
    public float TransitionIn() => TransitionIn(Duration);

    /// <summary>Starts an "in" transition using a custom duration.</summary>
    /// <remarks>If a negative value is passed, the default duration is used.</remarks>
    /// <param name="time">The duration of the transition in seconds. Negative values are overriden by the default value.</param>
    /// <returns>The actual duration of the transition in seconds.</returns>
    public float TransitionIn(float time) => Transitionate(time, false);

    /// <summary>Starts an "out" transition using the default duration.</summary>
    /// <returns>The actual duration of the transition in seconds.</returns>
    public float TransitionOut() => TransitionOut(Duration);

    /// <summary>Starts an "out" transition using a custom duration.</summary>
    /// <remarks>If a negative value is passed, the default duration is used.</remarks>
    /// <param name="time">The duration of the transition in seconds. Negative values are overriden by the default value.</param>
    /// <returns>The actual duration of the transition in seconds.</returns>
    public float TransitionOut(float time) => Transitionate(time, true);

    /// <summary>Overridable method to perform the transition.</summary>
    /// <remarks>Remember to call <c>base.OnTransitionStart()</c> at the start of the transition and <c>base.OnTransitionFinish()</c> for controlled behavior.</remarks>
    /// <param name="time">The duration of the transition in seconds.</param>
    /// <param name="backwards">If true, performs the transition in reverse.</param>
    /// <returns>The actual duration of the transition in seconds.</returns>
    protected virtual float PerformTransition(float time, bool backwards) => time;

    /// <summary>Resets the transition to its initial state.</summary>
    public virtual void Reset() { }

    protected virtual void OnTransitionStart()
    {
        IsTransitioning = true;
        TransitionStarted?.Invoke();
    }

    protected virtual void OnTransitionFinish()
    {
        IsTransitioning = false;
        TransitionFinished?.Invoke();
    }
}
