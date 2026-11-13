using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "JumpPadConfig", menuName = "Scriptable Objects/JumpPadConfig")]
public class JumpPadConfig : ScriptableObject
{
	[field: Header("Jumping")]
	[field: SerializeField, Range(0, 7)] public int Reach { get; private set; } = 5;
	[field: SerializeField, Range(0, 1)] public float PushDelay { get; private set; } = 0.38f;
	[field: SerializeField] public float PushVelocity { get; private set; } = 4.2f;
	[field: SerializeField] public Vector2 JumpPowerMinMax { get; private set; }


	[field: Header("General")]
	[field: SerializeField] public float ChangeDirectionTime { get; private set; } = 0.4f;
	[field: SerializeField] public LayerMask ObstaclesLayer { get; private set; }
	[field: SerializeField] public float ReactivateAnimPercent { get; private set; } = 0.75f;


	[field: Header("Sound")]
	[field: SerializeField] public AudioClip Sound { get; private set; }
	[field: SerializeField] public float BaseVolume { get; private set; } = 0.85f;
	[field: SerializeField] public float MaxAdditionalVolume { get; private set; } = 0.7f;
	[field: SerializeField] public Vector2 BasePitchRange { get; private set; } = new(-0.25f, -0.15f);
	[field: SerializeField] public float MaxAdditionalPitch { get; private set; } = 0.4f;
	[field: SerializeField] public Vector2 FadeTime { get; private set; } = new(0.1f, 0.3f);


	[field: Header("Motion Entity")]
	[field: SerializeField] public float ModelRotationTime { get; private set; } = 1.12f;
	[field: SerializeField] public float EntityMaxDistance { get; private set; } = 0.3f;
	[field: SerializeField] public Ease RotateEasing { get; private set; }

}
