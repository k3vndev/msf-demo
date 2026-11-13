using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardSchemer : MonoBehaviour
{
    public enum Scheme { Both, Arrows, WASD }
    public Scheme CurrentScheme { get; private set; }

    public static event Action<Scheme> OnSwap;

    void Awake()
    {
        SwapControlScheme(Scheme.Both);
    }

    void SwapControlScheme(Scheme scheme)
    {
        OnSwap?.Invoke(scheme);
        CurrentScheme = scheme;
    }

    private void OnInputDetectorChangeDevice(InputDeviceDetector.Device currentDevice)
    {
        if (currentDevice == InputDeviceDetector.Device.Gamepad)
            SwapControlScheme(Scheme.Both);
    }

    void OnGameplayMovePerformed(InputAction.CallbackContext context)
    {
        string key = context.control.name;
        static bool IsInKeys(string key, string[] keys) => keys.Contains(key.ToLower());

        if (IsInKeys(key, Arrows.Keys) && CurrentScheme != Scheme.Arrows)
            SwapControlScheme(Scheme.Arrows);

        else if (IsInKeys(key, WASD.Keys) && CurrentScheme != Scheme.WASD)
            SwapControlScheme(Scheme.WASD);
    }

    void OnGameplayActionPerformed(InputAction.CallbackContext context)
    {
        string key = context.control.name.ToLower();

        if (key == Arrows.Action && CurrentScheme != Scheme.Arrows)
            SwapControlScheme(Scheme.Arrows);

        else if (key == WASD.Action && CurrentScheme != Scheme.WASD)
            SwapControlScheme(Scheme.WASD);
    }

    public void OnEnable()
    {
        InputDeviceDetector.OnChangeDevice += OnInputDetectorChangeDevice;
        InputManager.Gameplay.Move.performed += OnGameplayMovePerformed;
        InputManager.Gameplay.Action.performed += OnGameplayActionPerformed;
    }

    public void OnDisable()
    {
        InputDeviceDetector.OnChangeDevice -= OnInputDetectorChangeDevice;
        InputManager.Gameplay.Move.performed -= OnGameplayMovePerformed;
        InputManager.Gameplay.Action.performed -= OnGameplayActionPerformed;
    }

    static class WASD
    {
        public static readonly string[] Keys = { "w", "a", "s", "d" };
        public static readonly string Action = "leftbutton";
    }

    static class Arrows
    {
        public static readonly string[] Keys = { "uparrow", "leftarrow", "downarrow", "rightarrow" };
        public static readonly string Action = "z";
    }
}