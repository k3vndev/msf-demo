using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Base class for entities that can move and interact with the environment, such as the player and the sled. It handles common functionality such as checking if the entity is grounded, keeping track of colliders that are currently touching the entity, and providing virtual methods for movement and rotation that can be overridden by derived classes.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class MotionEntity : MonoBehaviour
{
	public Rigidbody Rb { get; private set; }

	/// <summary>Whether the entity is currently touching the ground or not.</summary>
	public bool IsGrounded { get; private set; } = false;

	/// <summary>Invoked when the entity's grounded state changes.</summary>
	public event Action<bool> IsGroundedChanged;

	/// <summary>
	/// The transform of the model that will be rotated when the entity rotates. It should be a child of the main transform and a parent of the entity's mesh.
	/// </summary>
	[field: SerializeField] public Transform MeshModel { get; private set; }
	[field: SerializeField] public SFXPlayer SFXPlayer { get; private set; }
	[SerializeField, Range(0, 1)] float bouncyness = 0.25f;

	[Header("Motion Entity Particles")]
	[SerializeField] HitGroundParticles hitGroundParticles;

	readonly Vector3 groundCheckerOffset = new(0, -0.5f, 0);
	readonly float groundCheckerSize = 0.15f;
	string stretchTweenId;

	/// <summary>
	/// A set of all colliders that are currently touching the entity.
	/// </summary>
	public HashSet<Collider> CurrentColliders { get; private set; } = new();

	/// <summary>A set of all colliders that were touching the entity in the previous update (before the current list was updated). This is used to compare with the current colliders list and determine which colliders have been entered, stayed and exited.</summary>
	public HashSet<Collider> PreviousColliders { get; private set; } = new();

	/// <summary>Invoked when the list of current colliders changes.</summary>
	public event Action CurrentCollidersChanged;

	protected virtual void Awake()
	{
		Rb = GetComponent<Rigidbody>();
		stretchTweenId = $"Stretch_{gameObject.GetInstanceID()}";
	}

	protected virtual void Start() { }

	protected virtual void Update() { }

	void HandleIsGroundedChecking()
	{
		// Chek if the entity is currently grounded
		Vector3 center = transform.position + groundCheckerOffset;
		int mask = LayerMask.GetMask(LayerMasks.Snow.Name, LayerMasks.Pressables.Name);
		bool isCurrentlyGrounded = Utils.IsColliding(center, mask, groundCheckerSize);

		// If the grounded state has changed, call the OnIsGroundedChange method
		if (isCurrentlyGrounded != IsGrounded)
			OnIsGroundedChange(isCurrentlyGrounded);

		// Update the grounded global state
		IsGrounded = isCurrentlyGrounded;
	}

	public virtual void SnapPosition() { }

	public virtual void MoveTowards(Vector3 target, bool rotateModel = false) { }
	public virtual void MoveTowards(Vector2 target, bool rotateModel = false)
	{
		Vector3 m_target = new(target.x, transform.position.y, target.y);
		MoveTowards(m_target, rotateModel);
	}

	public virtual void RotateModel(Vector2 target, float time, Ease? ease = null, Action onFinish = null) { }

	/// <summary>Called when the grounded state of the entity changes.</summary>
	protected virtual void OnIsGroundedChange(bool isGrounded)
	{
		IsGroundedChanged?.Invoke(isGrounded);
		StretchAnimation(!isGrounded);
	}

	void StretchAnimation(bool isStretching)
	{
		if (bouncyness == 0f) return;
		Vector2 stretchTimeStartEnd = new(0.9f, 0.15f);

		var (targetScale, time, ease) = isStretching
			? (1f + bouncyness, stretchTimeStartEnd.x, Ease.OutQuart)
			: (1f, stretchTimeStartEnd.y, Ease.OutBack);

		DOTween.Kill(stretchTweenId);
		MeshModel
			.DOScaleY(targetScale, time)
			.SetId(stretchTweenId)
			.SetEase(ease);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		Vector3 center = transform.position + groundCheckerOffset;
		Gizmos.DrawWireCube(center, Vector3.one * groundCheckerSize);
	}

	public virtual void HitGroundFromJumpPad()
	{
		if (hitGroundParticles != null)
			hitGroundParticles.Play();

		if (SFXPlayer != null)
		{
			AudioClip audioClip = GameManager.I.StaticConfig.Audio.Clips.FallOnSnow;
			SFXPlayer.PlaySound(audioClip)?
				.SetPitchRange(0.14f, 0.35f)
				.SetFadeTime(0, 0.1f);
		}
	}

	protected virtual void OnCurrentCollidersRefresh()
	{
		CurrentCollidersChanged?.Invoke();
		HandleIsGroundedChecking();
	}

	// -- Collision and trigger handlers to keep track of the current colliders list --

	void OnCollisionEnter(Collision collision)
	{
		PreviousColliders = new HashSet<Collider>(CurrentColliders);
		CurrentColliders.Add(collision.collider);
		OnCurrentCollidersRefresh();
	}
	void OnCollisionExit(Collision collision)
	{
		PreviousColliders = new HashSet<Collider>(CurrentColliders);
		CurrentColliders.Remove(collision.collider);
		OnCurrentCollidersRefresh();
	}
	void OnTriggerEnter(Collider other)
	{
		PreviousColliders = new HashSet<Collider>(CurrentColliders);
		CurrentColliders.Add(other);
		OnCurrentCollidersRefresh();
	}
	void OnTriggerExit(Collider other)
	{
		PreviousColliders = new HashSet<Collider>(CurrentColliders);
		CurrentColliders.Remove(other);
		OnCurrentCollidersRefresh();
	}
}
