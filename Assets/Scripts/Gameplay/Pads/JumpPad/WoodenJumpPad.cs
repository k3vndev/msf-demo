
public class WoodenJumpPad : JumpPad
{
	protected override void OnEnable()
	{
		base.OnEnable();
		MechanismMaterials.Wood.OnToggle += OnToggleWood;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MechanismMaterials.Wood.OnToggle -= OnToggleWood;
	}

	private void OnToggleWood(bool value)
	{
		Direction newDirection = value ? SecondaryDirection : PrimaryDirection;
		ChangeDirection(newDirection);
	}
}
