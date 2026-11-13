using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Scriptable Objects/PlayerMovementConfig")]
public class PlayerMovementConfig : ScriptableObject
{
	[field: Header("Motion")]
	[field: SerializeField] public float SoloRotationTime { get; private set; } = 0.47f;
	[field: SerializeField] public float CarryingRotationTime { get; private set; } = 0.33f;
	[field: SerializeField] public float AccelerationTime { get; private set; } = 0.07f;
	[field: SerializeField] public float Speed { get; private set; } = 3.81f;
	[field: SerializeField] public float RotationFinishMinAngle { get; private set; } = 1.5f;

	[field: Header("Path Tracing")]
	[field: Tooltip("The max distance from the next path point to set a new one.")]
	[field: SerializeField, Range(0, 0.5f)] public float TracePathThreshold { get; private set; } = 0.33f;

	[field: Tooltip("The minimum distance from the next path point before returning to the previous one if user input stops.")]
	[field: SerializeField, Range(0, 1)] public float TracePathReturnThreshold { get; private set; } = 0.8f;
}
