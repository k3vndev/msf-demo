using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "SlidableEntityConfig", menuName = "Scriptable Objects/SlidableEntityConfig")]
public class SlidableEntityConfig : ScriptableObject
{
	[field: SerializeField] public float NextPointMinDistance { get; private set; } = 0.15f;
	[field: SerializeField] public float Speed { get; private set; } = 4.9f;
	[field: SerializeField] public float RotationTime { get; private set; } = 0.55f;
	[field: SerializeField] public Ease RotationEasing { get; private set; } = Ease.OutBack;
	[field: SerializeField] public bool StopOnHitEntity { get; private set; } = true;
	[field: SerializeField] public LayerMask EntitiesMask { get; private set; }
	[field: SerializeField] public LayerMask SolidsMask { get; private set; }

	[field: Header("Wall Hit")]
	[field: SerializeField] public AudioClip WallHit { get; private set; }
	[field: SerializeField] public GameObject WallHitParticlesObject { get; private set; }
	[field: SerializeField, Range(0f, 1f)] public float WallHitParticlesWallDistance { get; private set; } = 0.66f;
}
