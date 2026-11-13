using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BurningPad : TimedPad
{
	[Header("Burning Pad")]
	[SerializeField] Renderer meshRenderer;
	Color activeColor;
	[SerializeField] Color inactiveColor;
	[SerializeField] float colorTransitionTime = 0.31f;
	[SerializeField] Light pointLight;
	float initialLightIntensity;
	[SerializeField] SFXPlayer sfxPlayer;

	[Header("Blinking")]
	[SerializeField] int blinksCount;
	[SerializeField] AnimationCurve blinksCurve;
	[SerializeField] float blinkTransitionTime = 0.14f;
	[SerializeField] float blinkHoldTime = 0.24f;

	[Header("Blink Factor")]
	[SerializeField] Vector2 colorFactorMinMax;
	[SerializeField] Vector2 intensityFactorMinMax;

	[Header("Sounds")]
	[SerializeField] AudioClip burningSound;
	[SerializeField] AudioClip fireStartSound;
	[SerializeField] AudioClip fireEndSound;
	SoundHandler burningLoopSoundHandler;

	[Header("Smoke Particles")]
	[SerializeField] ParticleSystem smokeParticles;
	[SerializeField] float smokeEmissionRate = 2f;
	[SerializeField] float smokeDuration = 3f;
	[SerializeField] AnimationCurve smokeDurationCurve;

	Coroutine intensityLoopCoroutine;

	protected override void Awake()
	{
		base.Awake();

		meshRenderer.material = new Material(meshRenderer.material);
		activeColor = meshRenderer.material.color;
		initialLightIntensity = pointLight.intensity;
	}

	protected override void Start()
	{
		base.Start();
		StartEndBurningLoopSound(true);
		intensityLoopCoroutine = StartCoroutine(LoopIntensity(false));
	}

	protected override void Update()
		=> base.Update();

	public override void PlayerPress()
	{
		base.PlayerPress();

		if (PadIsActive)
		{
			if (PlayerController.CurrentSize == PlayerController.Size.Big)
			{
				StartCoroutine(PlayerPressSequence());
				SetPadIsActive(false);
				return;
			}

			// TODO: Handle player death
			Debug.LogWarning("Player died!");
		}
	}

	protected IEnumerator PlayerPressSequence()
	{
		// Shrink the player and stop movement
		PlayerController.ChangeSize(PlayerController.Size.Small);

		// Play extinguishing sound and stop the burning loop sound
		sfxPlayer.PlaySound(fireEndSound)?
			.SetFadeTime(0.1f, 0.9f);

		StartEndBurningLoopSound(false);
		StartCoroutine(PlaySmokeParticles());

		// Wait for the player to stop moving
		yield return StopPlayerMovement();

		// Set the pad to inactive state, refresh the pad's appearance, and reactivate it
		if (intensityLoopCoroutine != null) StopCoroutine(intensityLoopCoroutine);
		StartCoroutine(AnimateColorAndIntensity(0, 0, colorTransitionTime));
		Reactivate();

		// Allow the player to move again after a short delay
		yield return new WaitForSeconds(StopPlayerTime);
		PlayerMovement.EnableMovement();
	}

	IEnumerator AnimateColorAndIntensity(float colorFactor, float intensityFactor, float time)
	{
		Color startColor = meshRenderer.material.color;
		Color targetColor = Color.Lerp(inactiveColor, activeColor, colorFactor);

		float startIntensity = pointLight.intensity;
		float targetIntensity = Mathf.Lerp(0, initialLightIntensity, intensityFactor);

		float t = 0f;
		bool done = false;

		// Lerp the color and intensity over the specified duration
		DOTween.To(
			getter: () => t,
			setter: x =>
			{
				t = x;
				meshRenderer.material.color = Color.Lerp(startColor, targetColor, t);
				pointLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
			},
			endValue: 1f,
			duration: time
			)
			.SetEase(Ease.OutQuad)
			.OnComplete(() => done = true);

		yield return new WaitUntil(() => done);
	}

	protected override IEnumerator ReactivationSequence(float delay)
	{
		float startTime = Time.time;

		// Perform the blinking effect for the specified number of blinks
		for (int i = 1; i <= blinksCount; i++)
		{
			// Calculate the normalized time (t) for the current blink
			float t = i / (float)blinksCount;
			float blinkTime = blinksCurve.Evaluate(t);

			// Wait until the calculated blink time has passed
			yield return new WaitUntil(() => Time.time >= startTime + blinkTime * delay);

			// Calculate the color factor
			float colorFactor = Mathf.Lerp(colorFactorMinMax.x, colorFactorMinMax.y, t);
			float intensityFactor = Mathf.Lerp(intensityFactorMinMax.x, colorFactorMinMax.y, t);

			// Transition to the blink color, hold, and transition back to the inactive color
			yield return AnimateColorAndIntensity(colorFactor, intensityFactor, blinkTransitionTime);

			sfxPlayer.PlaySound(fireStartSound)?
				.SetFadeTime(0.1f, 0.9f)
				.SetPitchRange(t)
				.SetVolume(0.07f * t);

			yield return new WaitForSeconds(blinkHoldTime);
			yield return AnimateColorAndIntensity(0, intensityFactor, blinkTransitionTime);
		}

		// Wait until the total delay time has passed
		yield return new WaitUntil(() => Time.time >= startTime + delay);

		// Play the fire start sound and restart the burning loop sound
		sfxPlayer.PlaySound(fireStartSound)?
			.SetFadeTime(0.1f, 0.9f)
			.SetPitchRange(0.9f, 1.2f)
			.SetVolume(0.8f);

		StartEndBurningLoopSound(true);

		// Transition back to the active color
		meshRenderer.material
			.DOColor(activeColor, blinkTransitionTime)
			.SetEase(Ease.OutQuad);

		intensityLoopCoroutine = StartCoroutine(LoopIntensity(true));

		IsReactivating = false;
		SetPadIsActive(true);
	}

	IEnumerator LoopIntensity(bool startWithMax)
	{
		float maxIntensity = initialLightIntensity * 1.8f;
		float targetIntensity = startWithMax ? maxIntensity : initialLightIntensity;
		float stepDuration = 0.25f;

		while (true)
		{
			float elapsedTime = 0f;

			while (elapsedTime < stepDuration)
			{
				elapsedTime += Time.deltaTime;
				float t = elapsedTime / stepDuration;
				pointLight.intensity = Mathf.Lerp(initialLightIntensity, targetIntensity, t);
				yield return null;
			}
			targetIntensity = Random.Range(initialLightIntensity, maxIntensity);
			stepDuration = Random.Range(0.2f, 0.3f);
		}
	}

	void StartEndBurningLoopSound(bool isStarting)
	{
		if (isStarting)
		{
			burningLoopSoundHandler = sfxPlayer
				.PlaySound(burningSound)?
				.SetVolume(0.075f)
				.SetFadeTime(0.5f)
				.SetPitchRange(0.1f)
				.EnableLooping();

			return;
		}
		sfxPlayer.StopSound(burningLoopSoundHandler);
	}

	IEnumerator PlaySmokeParticles()
	{
		smokeParticles.Clear();
		smokeParticles.Play();

		ParticleSystem.EmissionModule emissionModule = smokeParticles.emission;
		emissionModule.rateOverTime = smokeEmissionRate;

		float elapsedTime = 0f;

		while (elapsedTime < smokeDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = smokeDurationCurve.Evaluate(elapsedTime / smokeDuration);
			emissionModule.rateOverTime = Mathf.Lerp(smokeEmissionRate, 0f, t);
			yield return null;
		}

		emissionModule.rateOverTime = 0f;
		smokeParticles.Stop();
	}
}
