using EZhex1991.EZSoftBone;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
	[SerializeField] SFXPlayer sfxPlayer;

	[Header("Shrink")]
	[SerializeField] AudioClip shrinkSound;
	[SerializeField] ParticleSystem shrinkParticles;

	[Header("Soft Bone Collisions")]
	[SerializeField] GameObject[] bodyColliders;
	[SerializeField] EZSoftBone hatSoftBone;
	[SerializeField] float hatSoftBoneDisableTime = 0.1f;

	PlayerMovement playerMovement;
	SledController sledController;
	Animator animator;

	public bool IsPlayingChangeSizeAnimation { get; private set; } = false;
	readonly float MIN_MOVEMENT_VELOCITY = 0.25f;

	/// <summary>Whether the player is visually changing its position or not.</summary>
	public bool IsMoving
	{
		get => playerMovement.IsMoving
			&& Mathf.Abs(playerMovement.AxisVelocity) > MIN_MOVEMENT_VELOCITY;
	}

	void Awake()
	{
		playerMovement = GetComponent<PlayerMovement>();
	}

	void Start()
	{
		animator = playerMovement.MeshModel.GetComponent<Animator>();
		sledController = Utils.GetComp<SledController>("Sled");
	}

	void Update()
	{
		animator.SetBool(Animations.Moving, IsMoving);
		animator.SetBool(Animations.Carrying, sledController.IsBeingCarried);
	}

	private void HandlePlayerChangeSize(PlayerController.Size newSize)
	{
		bool isGrowing = newSize == PlayerController.Size.Big;

		// Executes the desired animation for the player size change
		string triggerName = isGrowing ? Animations.Grow : Animations.Shrink;
		animator.SetTrigger(triggerName);

		// Manage collider gameobjects and disable them if not needed 
		foreach (GameObject obj in bodyColliders)
			obj.SetActive(isGrowing);

		if (isGrowing)
		{
			// Disable hat soft bone for a brief moment on grow
			hatSoftBone.enabled = false;

			Utils.Time.After(hatSoftBoneDisableTime, () =>
				hatSoftBone.enabled = true
			);
		}
		else
		{
			sfxPlayer.PlaySound(shrinkSound)?
				.SetPitchRange(-0.5f, -0.1f);

			shrinkParticles.Clear();
			shrinkParticles.Play();
		}
	}

	private void OnEnable()
		=> PlayerController.OnPlayerChangeSize += HandlePlayerChangeSize;

	private void OnDisable()
		=> PlayerController.OnPlayerChangeSize -= HandlePlayerChangeSize;

	static class Animations
	{
		public const string
			Moving = "is-moving",
			Carrying = "is-carrying",
			Grow = "grow",
			Shrink = "shrink";
	}
}
