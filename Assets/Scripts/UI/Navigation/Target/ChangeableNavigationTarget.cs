using System;
using UnityEngine;

public abstract class ChangeableNavigationTarget : NavigationTarget
{
    [field: Space]
    [field: SerializeField] public AnimationCurve ChangeValueCurve { get; private set; }

    /// <summary>Changes the value based on the given direction.</summary>
    /// <param name="direction">The change value direction. Can only be either -1 or 1.</param>
    /// <param name="looseValue">The proportion of time that was lost due to low framerate. Its calculated via x over deltaTime, where x is the time expected to be waited for in this holding loop.</param>
    public virtual void ChangeValue(int direction, float looseValue)
    {
        if (direction != 1 && direction != -1)
        {
            string msg = $"A different direction than 1 or -1 was given to ChangeValue(int) at {name} GameObject";
            throw new ArgumentOutOfRangeException(nameof(direction), msg);
        }
    }

    /// <summary>Refreshes the value display of the <c>ChangeableNavigationTarget</c> with the latest value.</summary>
    public virtual void RefreshValueDisplay() { }
}