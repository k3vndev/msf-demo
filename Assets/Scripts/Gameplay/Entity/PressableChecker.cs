using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class provides an object to be used for both Player and Sled to check for their pressable objects and invoke the corresponding methods on them.
/// </summary>
public class PressableChecker
{
    readonly MotionEntity motionEntity;
    readonly HashSet<Collider> currentColliders = new();
    readonly HashSet<Collider> previousColliders = new();

    public PressableChecker(MotionEntity motionEntity, HashSet<Collider> currentColliders, HashSet<Collider> previousColliders)
    {
        this.motionEntity = motionEntity;
        this.currentColliders = currentColliders;
        this.previousColliders = previousColliders;
    }

    /// <summary>
    /// Executes the main logic, finding which pressable objects have been entered, stayed and exited, and invoking the corresponding methods on them.
    /// </summary>
    public void Execute()
    {
        foreach (var collider in currentColliders)
        {
            if (!previousColliders.Contains(collider))
            {
                GetPlayerPressable(collider)?.PlayerPress();
                GetSledPressable(collider)?.SledPress();
            }
        }

        foreach (var collider in previousColliders)
        {
            if (!currentColliders.Contains(collider))
            {
                GetPlayerPressable(collider)?.PlayerRelease();
                GetSledPressable(collider)?.SledRelease();
            }
        }
    }

    IPlayerPressable GetPlayerPressable(Collider collider)
    {
        if (
            motionEntity is PlayerMovement
            && collider.gameObject.GetComponent<IPlayerPressable>() is IPlayerPressable playerPressable)
        {
            return playerPressable;
        }
        return null;
    }

    ISledPressable GetSledPressable(Collider collider)
    {
        if (
            motionEntity is SledController
            && collider.gameObject.GetComponent<ISledPressable>() is ISledPressable sledPressable)
        {
            return sledPressable;
        }
        return null;
    }
}