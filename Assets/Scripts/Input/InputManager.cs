using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages input contexts and provides access to gameplay and menu actions through Unity's input system.
/// </summary>
public class InputManager : MonoBehaviour
{
	public static InputManager I { get; private set; }

	/// <summary>Input actions specific to the gameplay context.</summary>
	public static Controls.GameplayActions Gameplay { get; private set; }

	/// <summary>Input actions specific to the menu context.</summary>
	public static Controls.MenuActions Menu { get; private set; }

	/// <summary>The generated input actions asset.</summary>
	public Controls Controls { get; private set; }

	public bool IsUsingGameplay { get; private set; } = true;
	public KeyboardSchemer KeyboardSchemeSwaper { get; private set; }
	public InputDeviceDetector Detector { get; private set; }

	void Awake()
	{
		if (I != null && I != this)
		{
			Destroy(gameObject);
			return;
		}
		I = this;

		Controls = new();

		Menu = Controls.Menu;
		Gameplay = Controls.Gameplay;

		KeyboardSchemeSwaper = GetComponent<KeyboardSchemer>();
		Detector = GetComponent<InputDeviceDetector>();
	}

	/// <summary>Enables the menu input map and disables the gameplay input map.</summary>
	public static void UseMenu()
	{
		Menu.Enable();
		Gameplay.Disable();
		I.IsUsingGameplay = false;
	}

	/// <summary>Enables the gameplay input map and disables the menu input map.</summary>
	public static void UseGameplay()
	{
		Gameplay.Enable();
		Menu.Disable();
		I.IsUsingGameplay = true;
	}

	/// <summary>
	/// Processes the raw input vector and determines the appropriate directional output.
	/// </summary>
	/// <returns>
	/// A processed <see cref="Vector2"/> based on the input:
	/// <list type="bullet">
	/// <item><description>If the magnitude of the input is less than the dead zone threshold, returns a zero vector.</description></item>
	/// <item><description>If both axes of the input have the same absolute value, returns <c>null</c>.</description></item>
	/// <item><description>Otherwise, returns a directional vector based on the input.</description></item>
	/// </list>
	/// </returns>
	public static Vector2? ProcessInput(Vector2 raw)
	{
		// If its within dead zone, return a zero magnitude vector
		if (raw.magnitude < Consts.AXIS_DEADZONE)
			return Vector2.zero;

		// If both axis are the same, return null
		Vector2 absInput = new(Mathf.Abs(raw.x), Mathf.Abs(raw.y));
		if (absInput.x == absInput.y) return null;

		return Utils.Vectors.ToDirectional(raw);
	}

	private void OnEnable() => Controls?.Enable();
	private void OnDisable() => Controls?.Disable();
}
