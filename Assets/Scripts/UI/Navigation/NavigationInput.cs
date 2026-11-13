using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

/// <summary>Manages input for navigation actions.</summary>
public class NavigationInput : MonoBehaviour
{
    [Header("Vertical (Navigating)")]
    [SerializeField, Range(0, 1)] float initialCooldownVer = 0.5f;
    [SerializeField, Range(0, 1)] float repeatingCooldownVer = 0.2f;

    [Header("Horizontal (Value Changing)")]
    [SerializeField, Range(0, 3)] float cooldownCurveHorTime = 1.3f;

    [Header("Pointer")]
    [SerializeField] float minPointerDelta = 0.001f;

    public static event Action<int> OnNavigate;
    /// <summary>
    /// Called when a <c>ChangeableNavigationTarget</c>'s value changes,
    /// passing its direction and loose value (expected wait time over delta).
    /// </summary>
    public static event Action<int, float> OnChangeValue;
    public static event Action OnAccept;
    public static event Action OnCancel;
    public static event Action<List<NavigationTarget>> OnPointerMoveOnTargets;
    public static event Action<List<NavigationTarget>, List<IPointerPressable>> OnPointerPress;
    public static event Action<List<NavigationTarget>, List<IPointerPressable>> OnPointerRelease;

    public bool IsHolding { get; private set; } = false;
    Vector2 lastInput = Vector2.zero;
    IEnumerator currentHoldingCoroutine;
    Vector2 lastPointerPos;

    public enum PointerInteraction { Press, Release }

    void Update()
    {
        // Handle holding
        Vector2 rawInput = InputManager.Menu.Navigate.ReadValue<Vector2>();

        if (IsHolding && InputManager.ProcessInput(rawInput) is Vector2 processedValue
            && processedValue.magnitude != 0 && processedValue != lastInput)
        {
            lastInput = processedValue;
            StartHolding();
        }

        // Handle pointer move
        if (!InputManager.I.IsUsingGameplay)
        {
            Vector2 currentPointerPos = GetPointerPosition();
            Vector2 delta = currentPointerPos - lastPointerPos;
            lastPointerPos = currentPointerPos;

            Vector2 normalizedDelta = new(
                x: delta.x / Screen.width,
                y: delta.y / Screen.height
            );

            if (normalizedDelta.magnitude >= minPointerDelta)
            {
                List<RaycastResult> results = RaycastAllAtPosition(currentPointerPos);
                List<NavigationTarget> targets = ExtractTargets(results);
                OnPointerMoveOnTargets?.Invoke(targets);
            }
        }
    }

    public void StartHolding()
    {
        if (lastInput.magnitude == 0) return;

        StopHolding();
        bool isNavigation = lastInput.y != 0;
        IsHolding = true;

        currentHoldingCoroutine = isNavigation
            ? HoldProcessNavigation()
            : HoldProcessValueChanging();

        StartCoroutine(currentHoldingCoroutine);
    }

    /// <summary>Forces to stop holding and stops any running holding coroutine.</summary>
    public void StopHolding()
    {
        if (currentHoldingCoroutine != null)
            StopCoroutine(currentHoldingCoroutine);

        IsHolding = false;
    }

    /// <summary>
    /// Handles the navigation input when a button is held down. 
    /// Invokes the <c>OnNavigated</c> event at a specified interval while the button is being held.
    /// </summary>
    IEnumerator HoldProcessNavigation()
    {
        int actionValue = GetActionValue();
        yield return new WaitForSecondsRealtime(initialCooldownVer);

        // While its holding, invoke the action and wait for repeating cooldown
        while (IsHolding)
        {
            OnNavigate?.Invoke(actionValue);
            yield return new WaitForSecondsRealtime(repeatingCooldownVer);
        }
    }

    /// <summary>
    /// Handles the navigation input when a button is held down. 
    /// Invokes the <c>OnValueChanged</c> event at a curve based interval while the button is being held.
    /// </summary>
    IEnumerator HoldProcessValueChanging()
    {
        if (NavigationSystem.I.CurrentSelected is not ChangeableNavigationTarget changeableTarget)
            yield break;

        AnimationCurve curve = changeableTarget.ChangeValueCurve;

        int actionValue = GetActionValue();
        float elapsed = 0;

        while (true)
        {
            // Get the value from the curve at the current time
            float x = curve.Evaluate(elapsed / cooldownCurveHorTime);

            // Increment elapsed time and make sure it doesn't go beyond max value
            if (elapsed < cooldownCurveHorTime)
                elapsed = Mathf.Min(elapsed + x, cooldownCurveHorTime);

            // Wait for x time and end process if the user release the button
            yield return new WaitForSecondsRealtime(x);
            float looseFactor = Time.unscaledDeltaTime / x;
            if (!IsHolding) break;

            // Invoke the action repeteadly
            OnChangeValue?.Invoke(actionValue, looseFactor);
        }
    }

    // Executes when the Menu.Navigate started, no matter if it's a tap or a hold interaction
    void OnNavigationStarted(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();
        if (InputManager.ProcessInput(rawInput) is Vector2 processedValue
            && processedValue.magnitude > 0)
        {
            bool isNavigation = processedValue.y != 0;

            if (isNavigation)
            {
                OnNavigate?.Invoke((int)processedValue.y);
                return;
            }
            OnChangeValue?.Invoke((int)processedValue.x, 1f);
        }
    }

    // Handle navigation hold interactions
    void OnNavigationPerformed(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();

        if (InputManager.ProcessInput(rawInput) is Vector2 processedValue
            && processedValue.magnitude > 0 && context.interaction is HoldInteraction)
        {
            // Start navigation holding process
            lastInput = processedValue;
            StartHolding();
        }
    }

    // Handle navigation holding cancelled
    void OnNavigationCanceled(InputAction.CallbackContext context)
    {
        if (context.interaction is HoldInteraction)
            StopHolding();
    }

    void OnCancelPerformed(InputAction.CallbackContext _) => OnCancel?.Invoke();

    void OnAcceptPerformed(InputAction.CallbackContext context) => OnAccept?.Invoke();

    int GetActionValue() => (int)(lastInput.y != 0 ? lastInput.y : lastInput.x);

    void OnPointerPressPerformed(InputAction.CallbackContext _)
    {
        PointerPressHandler(PointerInteraction.Press);
    }

    void OnPointerPressCanceled(InputAction.CallbackContext _)
    {
        PointerPressHandler(PointerInteraction.Release);
    }

    void PointerPressHandler(PointerInteraction interaction)
    {
        Vector2 pointerPosition = GetPointerPosition();
        List<RaycastResult> results = RaycastAllAtPosition(pointerPosition);

        // Handle navigation targets events
        List<NavigationTarget> navigationTargets = ExtractTargets(results);

        // Handle IPointerPressable's events
        List<IPointerPressable> pointerPressables = new();
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.TryGetComponent<IPointerPressable>(out var pressable))
                pointerPressables.Add(pressable);
        }

        // Invoke events
        if (interaction == PointerInteraction.Press)
        {
            OnPointerPress?.Invoke(navigationTargets, pointerPressables);
            return;
        }
        OnPointerRelease?.Invoke(navigationTargets, pointerPressables);
    }

    List<RaycastResult> RaycastAllAtPosition(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        PointerEventData pointerData = new(eventSystem) { position = position };

        List<RaycastResult> results = new();
        eventSystem.RaycastAll(pointerData, results);

        return results;
    }

    List<NavigationTarget> ExtractTargets(List<RaycastResult> results)
    {
        return results
            .Select(r => r.gameObject.GetComponent<NavigationTarget>())
            .Where(nt => nt != null)
            .ToList();
    }


    Vector2 GetPointerPosition() => InputManager.Menu.Point.ReadValue<Vector2>();



    void OnEnable()
    {
        InputManager.Menu.Navigate.started += OnNavigationStarted;
        InputManager.Menu.Navigate.performed += OnNavigationPerformed;
        InputManager.Menu.Navigate.canceled += OnNavigationCanceled;

        InputManager.Menu.Accept.performed += OnAcceptPerformed;
        InputManager.Menu.Cancel.performed += OnCancelPerformed;

        InputManager.Menu.Press.performed += OnPointerPressPerformed;
        InputManager.Menu.Press.canceled += OnPointerPressCanceled;
    }


    void OnDisable()
    {
        InputManager.Menu.Navigate.started -= OnNavigationStarted;
        InputManager.Menu.Navigate.performed -= OnNavigationPerformed;
        InputManager.Menu.Navigate.canceled -= OnNavigationCanceled;

        InputManager.Menu.Accept.performed -= OnAcceptPerformed;
        InputManager.Menu.Cancel.performed -= OnCancelPerformed;

        InputManager.Menu.Press.performed -= OnPointerPressPerformed;
        InputManager.Menu.Press.canceled -= OnPointerPressCanceled;
    }
}
