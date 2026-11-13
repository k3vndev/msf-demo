using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceDetector : MonoBehaviour
{
    readonly List<InputAction> actions = new();
    public enum Device { None, Gamepad, Keyboard, Mouse }

    public Device CurrentDevice { get; private set; } = Device.None;
    public static event Action<Device> OnChangeDevice;

    void Awake()
    {
        actions.AddRange(new InputAction[] {
            InputManager.Menu.Accept,
            InputManager.Menu.Navigate,
            InputManager.Menu.Cancel,
            InputManager.Gameplay.Action,
            InputManager.Gameplay.Move,
            InputManager.Gameplay.Pause
        });

        foreach (var action in actions)
            action.performed += OnAnyActionPerformed;
    }

    void OnAnyActionPerformed(InputAction.CallbackContext context)
    {
        if (!context.action.enabled) return;
        InputDevice device = context.control.device;

        if ((device is Gamepad || device is Joystick) && CurrentDevice != Device.Gamepad)
            SetDevice(Device.Gamepad);

        else if ((device is Keyboard) && CurrentDevice != Device.Keyboard)
            SetDevice(Device.Keyboard);

        else if ((device is Mouse) && CurrentDevice != Device.Mouse)
            SetDevice(Device.Mouse);
    }

    void SetDevice(Device device)
    {
        OnChangeDevice?.Invoke(device);
        CurrentDevice = device;
    }

    void OnDestroy()
    {
        foreach (var action in actions)
            action.performed -= OnAnyActionPerformed;
    }
}
