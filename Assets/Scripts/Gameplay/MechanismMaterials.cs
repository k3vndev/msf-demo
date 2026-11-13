using System;

public static class MechanismMaterials
{
	public static class Wood
	{
		/// <summary>
		/// Event triggered when the wood mechanism is toggled on or off.<br />
		/// The boolean parameter indicates the new active state.
		/// </summary>
		public static event Action<bool> OnToggle;

		/// <summary>
		/// Indicates whether the wood mechanism is currently active.
		/// </summary>
		public static bool IsActive { get; private set; } = false;

		/// <summary>
		/// Toggles the active state of the wood mechanism.<br />
		/// Triggers the <see cref="OnToggle"/> event with the new state.
		/// </summary>
		public static void Toggle()
		{
			IsActive = !IsActive;
			OnToggle?.Invoke(IsActive);
		}
	}

	public static class Stone
	{
		/// <summary>
		/// Event triggered when the active state of the stone mechanism changes.<br />
		/// The boolean parameter indicates the new active state.
		/// </summary>
		public static event Action<bool> OnChange;

		/// <summary>
		/// Tracks the number of entities currently overlapping the stone mechanism.
		/// </summary>
		public static int OverlappingEntitiesCount { get; private set; } = 0;

		/// <summary>
		/// Indicates whether the stone mechanism is currently active.
		/// </summary>
		public static bool IsActive { get; private set; } = false;

		/// <summary>
		/// Increases the overlapping entity count by one and updates the active state accordingly.
		/// </summary>
		public static void Activate()
		{
			OverlappingEntitiesCount++;
			Refresh();
		}

		/// <summary>
		/// Decreases the overlapping entity count by one (if greater than zero)
		/// and updates the active state accordingly.
		/// </summary>
		public static void Deactivate()
		{
			if (OverlappingEntitiesCount > 0) OverlappingEntitiesCount--;
			Refresh();
		}

		/// <summary>
		/// Re-evaluates the active state based on the number of overlapping entities.<br />
		/// Triggers the <see cref="OnChange"/> event if the state changes.
		/// </summary>
		static void Refresh()
		{
			if (OverlappingEntitiesCount <= 0 && IsActive)
			{
				IsActive = false;
				OnChange?.Invoke(false);
			}
			else if (OverlappingEntitiesCount > 0 && !IsActive)
			{
				IsActive = true;
				OnChange?.Invoke(true);
			}
		}
	}
}
