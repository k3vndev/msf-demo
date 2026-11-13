using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public abstract class JumpPad : Pad
{
	[Header("Configuration")]
	[field: SerializeField] public JumpPadConfig Config { get; private set; }
	[field: SerializeField] SFXPlayer sfxPlayer;

	[field: Header("Directions")]
	[field: SerializeField] protected Direction PrimaryDirection { get; private set; }
	[field: SerializeField] protected Direction SecondaryDirection { get; private set; }

	public bool IsUsingPrimaryDirection { get; protected set; } = true;

	/// <summary>
	/// Wheter another JumpPad can push and entity to this one or not.
	/// Is set to <c>true</c> when an entity steps on and to <c>false</c> as soon as it jumps away.
	/// </summary>
	public bool CanBeJumpedOn { get; private set; } = true;

	[Space, SerializeField] Transform meshModel;
	[SerializeField] Transform collisionHelper;

	Animator animator;
	MotionEntity currentEntity;

	const string ANIM_ACTIVATED = "activated";

	public event Action<JumpController> PushEntityStarted;
	public event Action<JumpController> EntityHitGround;

	protected override void Awake()
	{
		base.Awake();

		animator = GetComponentInChildren<Animator>();
	}

	protected override void Update()
		=> base.Update();

	public override void PlayerPress()
		=> HandleStartSequence(PlayerPressSequence());

	IEnumerator PlayerPressSequence()
	{
		yield return StopPlayerMovement();

		if (SledController.IsBeingCarried)
		{
			// Force the player to release the sled and snap it to the grid
			SledController.ForceStopCarrying();
			SledController.GridSnapToWorld();
		}

		yield return PushEntity(
			entity: PlayerMovement,
			rotate: true,
			onHitGround: () =>
			{
				PlayerMovement.SnapPosition();
				PlayerController.EnableInteraction();
				PlayerMovement.EnableMovement();
			});
	}

	public override void SledPress()
		=> HandleStartSequence(SledPressSequence());

	IEnumerator SledPressSequence()
	{
		if (SledController.IsBeingCarried)
		{
			PlayerMovement.DisableMovement();

			if (PlayerMovement.IsMoving && !PlayerMovement.IsRotating)
			{
				Vector2 direction = Utils.Vectors.CalcDirection(PlayerMovement.transform.position, transform.position);
				Vector3 offset = new(direction.x, 0, direction.y);

				// Ensure player moves until sled is over the jump pad
				Vector3 newPosition = transform.position - offset;
				PlayerMovement.MoveTowards(newPosition);
			}
			else
			{
				// Ensure player is grid snapped
				PlayerMovement.SnapPosition();
			}

			// Wait until player stops moving
			yield return new WaitUntil(() => !PlayerMovement.IsMoving);

			SledController.ForceStopCarrying();
			PlayerMovement.EnableMovement();

			// Align the sled with the jump pad
			SledController.MoveTowards(transform.position);
		}

		// Wait until sled is not being carried anymore
		yield return new WaitUntil(() =>
			!SledController.IsBeingCarried && !SledController.IsAdjustingPlayerRotation
			&& !SledController.IsAdjustingPosition && !SledController.IsAdjustingPlayerRotation);

		yield return PushEntity(
			entity: SledController,
			rotate: true,
			onHitGround: PlayerController.EnableInteraction
		);
	}

	void HandleStartSequence(IEnumerator sequence)
	{
		if (!CanBeSteppedOver || currentEntity != null) return;

		PlayerController.DisableInteraction();
		CanBeSteppedOver = false;
		CanBeJumpedOn = false;

		StartCoroutine(sequence);
	}

	IEnumerator PushEntity(MotionEntity entity, bool rotate = false, Action onHitGround = null)
	{
		StartCoroutine(HandleCurrentEntity(entity));
		JumpController jump = new(this);

		// Wait some time before pushing
		yield return new WaitForSeconds(Config.PushDelay);
		OnPushEntityStart(jump);

		// Stop sliding if the entity is a SlidableEntity
		SlidableEntity slidableEntity = entity as SlidableEntity;
		slidableEntity?.ForceStopSliding();

		if (rotate)
		{
			entity.RotateModel(
				target: jump.Direction,
				time: Config.ModelRotationTime,
				ease: Config.RotateEasing
			);
		}

		// Avoid physics position overriding
		entity.Rb.isKinematic = true;

		// Allow jumping on
		CanBeJumpedOn = true;

		// Perform jump
		entity.transform
			.DOJump(jump.Target, jump.JumpPower, 1, jump.Time)
			.SetEase(Ease.Linear);

		// Wait until jump is completed
		yield return new WaitForSeconds(jump.Time);
		entity.Rb.isKinematic = false;

		// Wait until the entity hits the ground
		yield return new WaitUntil(() => entity.IsGrounded);
		collisionHelper.gameObject.SetActive(false);

		// Check if the entity fell on the same JumpPad
		GameObject foundOtherJumpPad = entity.CurrentColliders
			.ToList()
			.Find(c => c.GetComponent<JumpPad>() != null)?
			.gameObject;

		if (foundOtherJumpPad == null || foundOtherJumpPad == gameObject)
		{
			onHitGround?.Invoke();

			// Request start sliding if the entity fell on the ground and is a SlidableEntity
			if (foundOtherJumpPad == null)
				slidableEntity?.RequestStartSliding(jump.Direction);
		}

		entity.HitGroundFromJumpPad();
		OnEntityHitGround(jump);
	}

	/// <summary>Called the exact moment the jump pad pushes an entity.</summary>
	/// <param name="jump">The jump data.</param>
	protected virtual void OnPushEntityStart(JumpController jump)
	{
		PushEntityStarted?.Invoke(jump);

		PlayPushSound(jump);

		// Play activation animation
		animator.SetBool(ANIM_ACTIVATED, true);
		SetPadIsActive(true);

		// Start wait timer for reseting pad
		float waitTime = jump.Target != transform.position ? ReactivationTime : 0;
		Reactivate(waitTime);
	}

	protected virtual void OnEntityHitGround(JumpController jump)
	{
		EntityHitGround?.Invoke(jump);
	}

	void PlayPushSound(JumpController jump)
	{
		// Calculate pitch range based on jump distance
		float normalizedDist = (float)jump.Distance / Config.Reach;
		float aditionalPitch = Config.MaxAdditionalPitch * normalizedDist;
		float volume = Config.BaseVolume + Config.MaxAdditionalVolume * normalizedDist;

		// Play push sound
		sfxPlayer
			.PlaySound(Config.Sound)?
			.SetVolume(volume)
			.SetPitchRange(Config.BasePitchRange.x + aditionalPitch, Config.BasePitchRange.y + aditionalPitch)
			.SetFadeTime(Config.FadeTime.x, Config.FadeTime.y);
	}

	protected override IEnumerator ReactivationSequence(float waitTime)
	{
		// Wait for reactivation time
		yield return new WaitForSeconds(waitTime);
		animator.SetBool(ANIM_ACTIVATED, false);

		// Wait until Contract animation finishes or state is on Idle
		yield return new WaitUntil(() =>
		{
			AnimatorStateInfo animatorState = animator.GetCurrentAnimatorStateInfo(0);
			bool contractAnimFinished = animatorState.IsName("Contract")
				&& animatorState.normalizedTime > Config.ReactivateAnimPercent;

			return contractAnimFinished || animatorState.IsName("Idle");
		});

		SetPadIsActive(false);
		CanBeSteppedOver = true;
		IsReactivating = false;
	}

	IEnumerator HandleCurrentEntity(MotionEntity entity)
	{
		currentEntity = entity;
		yield return new WaitUntil(() =>
		{
			float distance = Utils.Vectors.DistanceIgnoreY(transform.position, entity.transform.position);
			return distance > Config.EntityMaxDistance;
		});
		currentEntity = null;
	}

	public override void PlayerRelease() { }
	public override void SledRelease() { }

	protected void ChangeDirection(Direction direction)
	{
		if (direction == GetDirection()) return;
		IsUsingPrimaryDirection = direction == PrimaryDirection;

		float angle = GetAngleFromDirection();
		Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, angle, transform.eulerAngles.z);

		meshModel
			.DORotateQuaternion(targetRotation, Config.ChangeDirectionTime)
			.SetEase(Ease.OutCubic);
	}

	Direction GetDirection() => IsUsingPrimaryDirection ? PrimaryDirection : SecondaryDirection;
	float GetAngleFromDirection() => Utils.DirectionToAngle(GetDirection());

	public class JumpController
	{
		readonly public Vector3 Target;
		readonly public Vector2 Direction;
		readonly public float Time;
		readonly public float JumpPower;
		readonly public int Distance;

		readonly JumpPad jumpPad;
		readonly JumpPadConfig config;

		public JumpController(JumpPad jumpPad)
		{
			this.jumpPad = jumpPad;
			config = jumpPad.Config;

			Target = GetTargetPosition();
			Direction = Utils.Vectors.DegToVec(jumpPad.GetAngleFromDirection());
			Time = config.Reach / config.PushVelocity;
			Distance = Mathf.RoundToInt(Utils.Vectors.DistanceIgnoreY(Target, jumpPad.transform.position));
			JumpPower = Mathf.Lerp(config.JumpPowerMinMax.x, config.JumpPowerMinMax.y, Distance / config.Reach);
		}

		Vector3 GetTargetPosition()
		{
			// Extract the angle sign and a directional vector from the current direction
			int dir = (int)jumpPad.GetDirection();
			Vector3 directionalVector = dir % 2 != 0 ? Vector3.right : Vector3.forward;
			int sign = (int)-Mathf.Sign(dir - 2);

			// Check for valid spots from max reach to 1
			for (int i = config.Reach; i > 0; i--)
			{
				// Calculate position
				Vector3 position = jumpPad.transform.position + i * sign * directionalVector;
				position.y = 1;

				// Return the first free position
				if (!EntityWouldCollideAt(position))
				{
					SetCollisionHelper(position);
					return position;
				}
			}

			SetCollisionHelper(jumpPad.transform.position);
			return jumpPad.transform.position;
		}

		void SetCollisionHelper(Vector3 position)
		{
			jumpPad.collisionHelper.gameObject.SetActive(true);
			jumpPad.collisionHelper.position = position;
		}

		bool EntityWouldCollideAt(Vector3 position)
		{
			Physics.SyncTransforms();

			// Return true if it's outside level bounds
			if (!LevelManager.I.IsInsideBounds(position)) return true;

			// Return true if the entity is the player and next floor is ice 
			Vector3 floorPosition = position - Vector3.up;
			if (jumpPad.currentEntity is PlayerMovement && Utils.IsColliding(floorPosition, LayerMasks.Ice.Mask)) return true;

			// Return true if it's colliding with an obstacle
			if (Utils.IsColliding(position, config.ObstaclesLayer)) return true;

			// Return true if it's a Frame or another JumpPad that cannot be stepped over
			foreach (Collider collider in Utils.GetCollidersAt(position))
			{
				if (collider.CompareTag("Frame")) return true;

				collider.TryGetComponent(out Door door);
				if (door != null && !door.IsOpen) return true;

				collider.TryGetComponent(out JumpPad jumpPad);
				if (jumpPad != null && !jumpPad.CanBeJumpedOn) return true;
			}
			return false;
		}
	}

	void OnValidate()
	{
		meshModel.transform.rotation = Quaternion.Euler(0, GetAngleFromDirection(), 0);
	}
}
