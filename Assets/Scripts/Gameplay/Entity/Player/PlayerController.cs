using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public enum Size { Small, Big }
	public Size CurrentSize { get; private set; } = Size.Big;
	public bool SledIsOnRange { get; private set; } = false;
	public bool CanInteract { get; private set; } = true;
	public static event Action<Size> OnPlayerChangeSize;
	SledController sledController;
	PlayerInput input;
	PlayerMovement movement;

	private void Start()
	{
		sledController = Utils.GetComp<SledController>("Sled");
		movement = GetComponent<PlayerMovement>();
		input = GetComponent<PlayerInput>();
	}

	void Update()
	{
		HandleStartCarrying();
		HandleStopCarrying();
	}

	public void ChangeSize(Size newSize)
	{
		CurrentSize = newSize;
		OnPlayerChangeSize?.Invoke(CurrentSize);

		if (CurrentSize == Size.Small && sledController.IsBeingCarried)
		{
			sledController.StopCarrying();
		}
	}

	void HandleStartCarrying()
	{
		// Handle start carrying
		if (input.ActionKeyIsPressed && SledIsOnRange
			&& !sledController.IsBeingCarried && CurrentSize == Size.Big && CanInteract)
		{
			sledController.StartCarrying();
		}
	}

	void HandleStopCarrying()
	{
		if (!input.ActionKeyIsPressed && sledController.IsBeingCarried
			&& CanInteract && !movement.IsRotating)
		{
			sledController.StopCarrying();
		}
	}

	///<summary>Changes the value of SledIsOnRange. Beware at calling it.</summary>
	public void SetSledIsOnRange(bool value) => SledIsOnRange = value;

	///<summary>Allows the player to carry the sled.</summary>
	public void EnableInteraction() => CanInteract = true;

	///<summary>Prevents the player from carrying the sled.</summary>
	public void DisableInteraction() => CanInteract = false;
}
