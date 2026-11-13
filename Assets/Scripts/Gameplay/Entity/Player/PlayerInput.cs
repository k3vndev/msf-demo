using UnityEngine;

public class PlayerInput : MonoBehaviour
{
	public Vector2 Movement { get; private set; }
	public Axis Axis { get; private set; }
	public bool ActionKeyIsPressed { get; private set; }

	private void Update()
		=> SetMovementAndAxis();

	void SetMovementAndAxis()
	{
		Vector2 rawInput = InputManager.Gameplay.Move.ReadValue<Vector2>();

		if (InputManager.ProcessInput(rawInput) is Vector2 processedInput)
		{
			Movement = processedInput;

			if (Movement != Vector2.zero)
				Axis = Movement.x != 0 ? Axis.Horizontal : Axis.Vertical;
		}
	}

	private void OnActionPerformed(UnityEngine.InputSystem.InputAction.CallbackContext _)
		=> ActionKeyIsPressed = true;

	private void OnActionCancelled(UnityEngine.InputSystem.InputAction.CallbackContext _)
		=> ActionKeyIsPressed = false;

	private void OnEnable()
	{
		InputManager.Gameplay.Action.performed += OnActionPerformed;
		InputManager.Gameplay.Action.canceled += OnActionCancelled;
	}

	private void OnDisable()
	{
		InputManager.Gameplay.Action.performed -= OnActionPerformed;
		InputManager.Gameplay.Action.canceled -= OnActionCancelled;
	}
}

