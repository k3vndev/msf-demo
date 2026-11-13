using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationSystem : MonoBehaviour
{
    public static NavigationSystem I { get; private set; }
    readonly HashSet<NavigationTarget> currentTargets = new();

    public NavigationTarget CurrentSelected { get; private set; }
    public bool IsNavigating { get; private set; } = false;
    public bool CanNavigate { get; private set; } = true;

    /// <summary>
    /// Called when either <c>NavigationSystem.IsNavigating</c> or 
    /// <c>NavigationSystem.CurrentSelected</c> changes.
    /// </summary>
    public static event Action OnChangeState;

    /// <summary>Called when <c>NavigationSystem.CanNavigate</c> changes.</summary>
    public static event Action<bool> OnChangeCanNavigate;

    /// <summary>Called when user pressed cancel button and <c>NavigationSystem.CanNavigate</c> is <c>true</c>.</summary>
    public static event Action OnCancel;

    public NavigationInput Input { get; private set; }
    public bool IsUsingPointer { get; private set; } = false;

    public ButtonTarget CurrentHoldingButton { get; private set; }
    public static event Action<ButtonTarget> OnCurrentHoldingButtonChange;

    void Awake()
    {
        if (I != null && I != this)
        {
            gameObject.SetActive(false);
            Destroy(this);
            return;
        }
        I = this;

        Input = GetComponent<NavigationInput>();
    }

    /// <summary>Sets all the NavigationTargets found inside the given container as valid targets for the system.</summary>
    public void Initialize(GameObject targetsContainer)
    {
        currentTargets.Clear();

        // Set the current targets
        NavigationTarget[] foundTargets = targetsContainer.GetComponentsInChildren<NavigationTarget>();
        currentTargets.UnionWith(foundTargets);
    }

    /// <summary>Selects the given target.</summary>
    public void Select(NavigationTarget target)
    {
        NavigationTarget lastSelected = CurrentSelected;
        CurrentSelected = target;
        OnChangeState?.Invoke();

        if (lastSelected is ChangeableNavigationTarget
            && CurrentSelected is ChangeableNavigationTarget && Input.IsHolding)
        {
            // Reset the holding state if the target is changeable and the player is holding
            Input.StopHolding();
            Input.StartHolding();
        }
    }

    void HandleNavigate(int dir)
    {
        if (CanStartNavigatingFromButton())
        {
            NavigationTarget newSelected = CurrentSelected.Navigate(dir);
            if (newSelected != null) Select(newSelected);
        }
    }

    void HandleChangeValue(int dir, float looseValue)
    {
        if (CanStartNavigatingFromButton()
            && CurrentSelected is ChangeableNavigationTarget changeableSelected)
        {
            changeableSelected.ChangeValue(dir, looseValue);
        }
    }

    void HandleAccept()
    {
        if (CanStartNavigatingFromButton())
            CurrentSelected.Accept();
    }

    void HandleCancel()
    {
        if (CanNavigate) OnCancel?.Invoke();
    }

    /// <summary>Middleware for navigation actions and the responsible of starting navigations.</summary>
    /// <returns>Whether it can proceed with the action or not.</returns>
    bool CanStartNavigatingFromButton()
    {
        StopUsingPointer();

        if (!IsNavigating)
        {
            StartNavigating();
            return false;
        }
        return CanNavigate;
    }

    public void StartNavigating()
    {
        IsNavigating = true;
        OnChangeState?.Invoke();
    }

    public void StopNavigating()
    {
        IsNavigating = false;
        OnChangeState?.Invoke();
    }

    public void EnableNavigation()
    {
        CanNavigate = true;
        OnChangeCanNavigate?.Invoke(CanNavigate);
    }

    public void DisableNavigation()
    {
        CanNavigate = false;
        OnChangeCanNavigate?.Invoke(CanNavigate);
    }

    void StartUsingPointer()
    {
        IsUsingPointer = true;
        GameManager.SetCursorVisibility(true);
    }

    void StopUsingPointer()
    {
        IsUsingPointer = false;
        GameManager.SetCursorVisibility(false);
    }

    void HandlePointerMoveOnTargets(List<NavigationTarget> targets)
    {
        GameManager.SetCursorVisibility(true);
        NavigationTarget hitTarget = targets.FirstOrDefault(t => currentTargets.Contains(t));

        if (hitTarget && (hitTarget != CurrentSelected || !IsNavigating))
        {
            StartNavigating();
            Select(hitTarget);

            StartUsingPointer();
        }

        if (!hitTarget && IsNavigating && IsUsingPointer)
        {
            StopNavigating();
            return;
        }
    }

    void HandlePointerPressOnTargets(
        List<NavigationTarget> targets,
        NavigationInput.PointerInteraction pointerInteraction)
    {
        NavigationTarget hitTarget = targets.FirstOrDefault(t => currentTargets.Contains(t));

        if (hitTarget == null || !CanNavigate)
        {
            // Clear heldButton on pointer up with no targets
            if (!hitTarget && pointerInteraction == NavigationInput.PointerInteraction.Release)
            {
                SetCurrentHoldingButton(null);
            }
            return;
        }

        ButtonTarget buttonTarget = hitTarget as ButtonTarget;

        switch (pointerInteraction)
        {
            case NavigationInput.PointerInteraction.Press:
                {
                    if (buttonTarget)
                    {
                        SetCurrentHoldingButton(buttonTarget);
                        break;
                    }
                    hitTarget.Accept();

                    break;
                }
            case NavigationInput.PointerInteraction.Release:
                {
                    if (CurrentHoldingButton == buttonTarget)
                    {
                        CurrentHoldingButton?.Accept();
                    }
                    SetCurrentHoldingButton(null);
                    break;
                }
        }
    }


    void SetCurrentHoldingButton(ButtonTarget buttonTarget)
    {
        CurrentHoldingButton = buttonTarget;
        OnCurrentHoldingButtonChange?.Invoke(buttonTarget);
    }

    void OnPointerPress(List<NavigationTarget> targets, List<IPointerPressable> pressables)
    {
        StartUsingPointer();
        if (targets.Count == 0) StopNavigating();

        // Handle targets
        HandlePointerPressOnTargets(targets, NavigationInput.PointerInteraction.Press);

        // Handle pressables
        foreach (IPointerPressable pressable in pressables)
            if (CanNavigate) pressable.PointerPress();
    }

    void OnPointerRelease(List<NavigationTarget> targets, List<IPointerPressable> pressables)
    {
        StartUsingPointer();

        // Handle targets
        HandlePointerPressOnTargets(targets, NavigationInput.PointerInteraction.Release);

        // Handle pressables
        foreach (IPointerPressable pressable in pressables)
            if (CanNavigate) pressable.PointerRelease();
    }


    void OnEnable()
    {
        NavigationInput.OnNavigate += HandleNavigate;
        NavigationInput.OnChangeValue += HandleChangeValue;
        NavigationInput.OnAccept += HandleAccept;
        NavigationInput.OnCancel += HandleCancel;

        NavigationInput.OnPointerMoveOnTargets += HandlePointerMoveOnTargets;
        NavigationInput.OnPointerPress += OnPointerPress;
        NavigationInput.OnPointerRelease += OnPointerRelease;
    }

    void OnDisable()
    {
        NavigationInput.OnNavigate -= HandleNavigate;
        NavigationInput.OnChangeValue -= HandleChangeValue;
        NavigationInput.OnAccept -= HandleAccept;
        NavigationInput.OnCancel -= HandleCancel;

        NavigationInput.OnPointerMoveOnTargets -= HandlePointerMoveOnTargets;
        NavigationInput.OnPointerPress -= OnPointerPress;
        NavigationInput.OnPointerRelease -= OnPointerRelease;
    }
}
