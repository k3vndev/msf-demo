using System.Collections;
using UnityEngine;

public abstract class Pad : MonoBehaviour, IPlayerPressable, ISledPressable
{
	[field: SerializeField] protected float ReactivationTime { get; private set; }

	/// <summary>Wether a Pad can be stepped over or not.</summary>
	public bool CanBeSteppedOver { get; protected set; } = true;

	/// <summary>
	/// Wether a Pad is active or not.
	/// If set to <c>true</c>, the Pad will likely take effect as soon as an entity steps over it.
	/// </summary>
	public bool PadIsActive { get; private set; } = true;

	PlayerMovement m_playerMovement;
	PlayerController m_playerController;
	SledController m_sledController;

	protected PlayerMovement PlayerMovement
		=> m_playerMovement ??= Utils.GetComp<PlayerMovement>("Player");

	protected PlayerController PlayerController
		=> m_playerController ??= Utils.GetComp<PlayerController>("Player");

	protected SledController SledController
		=> m_sledController ??= Utils.GetComp<SledController>("Sled");

	protected bool IsReactivating = false;

	protected virtual void Awake() { }
	protected virtual void Start() { }
	protected virtual void Update() { }

	protected virtual void SetPadIsActive(bool value)
		=> PadIsActive = value;

	/// <summary>Stops the player and waits until is not moving at all.</summary>
	protected IEnumerator StopPlayerMovement()
	{
		PlayerMovement.DisableMovement();
		Vector3 snapPosition = Utils.Vectors.Round(transform.position);
		PlayerMovement.MoveTowards(snapPosition);

		yield return new WaitUntil(() =>
			!PlayerMovement.IsMoving && !PlayerMovement.IsRotating);
	}

	/// <summary>Reactivation sequence for the pad.</summary>
	/// <param name="delay">Delay before reactivation.</param>
	protected virtual IEnumerator ReactivationSequence(float delay) { yield break; }

	/// <summary>Reactivates the pad after the default delay.</summary>
	protected void Reactivate() => Reactivate(ReactivationTime);

	/// <summary>Reactivates the pad after a specified delay.</summary>
	/// <param name="delay">Delay before reactivation.</param>
	protected void Reactivate(float delay)
	{
		if (!IsReactivating)
		{
			IsReactivating = true;
			StartCoroutine(ReactivationSequence(delay));
		}
	}

	protected virtual void OnEnable() { }
	protected virtual void OnDisable() { }

	public virtual void SledPress() { }
	public virtual void SledRelease() { }
	public virtual void PlayerPress() { }
	public virtual void PlayerRelease() { }
}