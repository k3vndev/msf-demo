using System;
using DG.Tweening;
using UnityEngine;


/// <summary>Base class for all navigation targets.</summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class NavigationTarget : MonoBehaviour
{
    [field: SerializeField] protected Neighbours Neighbours { get; private set; }

    /// <summary>
    /// Indicates whether this navigation target is currently selected in the navigation system.
    /// This represents logical focus and determines which element receives input actions
    /// from keyboard or controller navigation.
    /// </summary>
    public bool IsSelected { get; private set; } = false;

    /// <summary>
    /// Indicates whether this navigation target is currently showing its "selected" visuals.
    /// This is purely a presentation state and may differ from <see cref="IsSelected"/>.
    /// For example, a target can be selected without being visually highlighted when the UI
    /// is in a non-navigation mode or when using mouse hover behavior.
    /// </summary>
    public bool IsVisuallySelected { get; private set; } = false;

    /// <summary>
    /// Called after <c>NavigationTarget.OnStateChange()</c> and passes as an argument the new value
    /// of <c>NavigationTarget.IsSelected</c> before it's even set.
    /// </summary>
    public event Action<bool> OnVisualsChanged;

    protected CanvasGroup CanvasGroup { get; private set; }
    protected RectTransform RectTransform { get; private set; }

    readonly float disabledAlpha = 0.66f;
    readonly float fadeTime = 0.14f;

    protected virtual void Awake()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
        CanvasGroup.alpha = NavigationSystem.I.CanNavigate ? 1 : disabledAlpha;
        RectTransform = GetComponent<RectTransform>();
    }

    protected virtual void Start() { }
    protected virtual void Update() { }

    public virtual void Accept() { }

    /// <summary>Navigate on the vertical axis to the given direction.</summary>
    /// <param name="direction">The navigation direction. Can only be either -1 or 1.</param>
    public virtual NavigationTarget Navigate(int direction)
    {
        // Prevent navigation if the direction is not valid
        if (Mathf.Abs(direction) != 1) return null;

        NavigationTarget target = direction == 1
            ? Neighbours.Top
            : Neighbours.Bottom;

        return target;
    }

    /// <summary>
    /// Event listener for <c>NavigationSystem.OnChangeState</c>. <br/>
    /// Triggered when either <c>NavigationSystem.IsNavigating</c> or <c>NavigationSystem.CurrentSelected</c> changes.
    /// </summary>
    protected virtual void OnChangeState()
    {
        bool newValue = NavigationSystem.I.CurrentSelected == this;
        IsSelected = newValue;

        // Only set visuals when it's navigating
        if (IsSelected && NavigationSystem.I.IsNavigating)
        {
            EnableSelectedVisuals();
            return;
        }
        DisableSelectedVisuals();
    }

    protected virtual void OnChangeCanNavigate(bool canNavigate)
    {
        float target = canNavigate ? 1 : disabledAlpha;

        CanvasGroup.DOKill();
        CanvasGroup
            .DOFade(target, fadeTime)
            .SetEase(Ease.OutSine)
            .SetUpdate(UpdateType.Normal, true);
    }

    public virtual void EnableSelectedVisuals()
    {
        IsVisuallySelected = true;
        OnVisualsChanged?.Invoke(true);
    }

    public virtual void DisableSelectedVisuals()
    {
        IsVisuallySelected = false;
        OnVisualsChanged?.Invoke(false);
    }

    protected virtual void OnEnable()
    {
        NavigationSystem.OnChangeState += OnChangeState;
        NavigationSystem.OnChangeCanNavigate += OnChangeCanNavigate;
    }

    protected virtual void OnDisable()
    {
        NavigationSystem.OnChangeState -= OnChangeState;
        NavigationSystem.OnChangeCanNavigate -= OnChangeCanNavigate;
    }
}
