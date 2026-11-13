using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SnowyPad : TimedPad
{
	[Header("Snowy Pad")]
	[SerializeField] GameObject meshUp;
	[SerializeField] GameObject meshDown;

	[Header("Animation")]
	[SerializeField] float animScaleY = 1.1f;
	[SerializeField] Vector2 animScaleTimeStartEnd;

	[Space]
	[SerializeField] ParticleSystem snowParticles;
	[SerializeField] AudioClip snowSound;
	[SerializeField] SFXPlayer sfxPlayer;

	protected override void Awake()
		=> base.Awake();

	protected override void Update()
		=> base.Update();

	public override void PlayerPress()
	{
		base.PlayerPress();
		if (!PadIsActive) return;

		SetPadIsActive(false);

		if (PlayerController.CurrentSize == PlayerController.Size.Small)
		{
			StartCoroutine(PlayerPressSequence());
			return;
		}
		Reactivate();
	}

	public override void SledPress()
	{
		base.SledPress();
		if (!PadIsActive) return;

		SetPadIsActive(false);
		Reactivate();
	}

	protected override void SetPadIsActive(bool newValue)
	{
		base.SetPadIsActive(newValue);

		// Toggle mesh visibility
		meshUp.SetActive(PadIsActive);
		meshDown.SetActive(!PadIsActive);

		if (PadIsActive)
		{
			// Animate mesh up
			meshUp.transform.DOKill();
			meshUp.transform
				.DOScaleZ(animScaleY, animScaleTimeStartEnd.x)
				.SetEase(Ease.OutSine)
				.OnComplete(() =>
				{
					meshUp.transform
						.DOScaleZ(1, animScaleTimeStartEnd.y)
						.SetEase(Ease.OutBack);
				});
		}

		// Play snow particles
		snowParticles.Play();

		// Play sound
		var pitchRange = PadIsActive
			? new Vector2(0.1f, 0.24f)
			: new Vector2(-0.5f, -0.2f);

		sfxPlayer.PlaySound(snowSound)?
			.SetVolume(1.5f)
			.SetFadeTime(0)
			.SetPitchRange(pitchRange.x, pitchRange.y);
	}

	protected IEnumerator PlayerPressSequence()
	{
		PlayerController.ChangeSize(PlayerController.Size.Big);
		Reactivate();

		yield return StopPlayerMovement();
		yield return new WaitForSeconds(StopPlayerTime);
		PlayerMovement.EnableMovement();
	}

	protected override IEnumerator ReactivationSequence(float delay)
	{
		yield return new WaitForSeconds(delay);
		IsReactivating = false;
		SetPadIsActive(true);
	}
}
