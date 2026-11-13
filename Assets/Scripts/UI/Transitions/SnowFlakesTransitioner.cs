using System.Collections;
using UnityEngine;

public class SnowFlakesTransitioner : Transitioner
{
    [Header("Implementation Config")]
    [SerializeField] AnimationCurve durationCurve;
    [SerializeField] float particlesExtraTime = 0.8f;
    [SerializeField] float maxParticleCountForFinishing = 20f;

    [Header("Fade")]
    [SerializeField] Vector2 fadeDelayInOut = new(1.55f, 0.5f);
    [SerializeField] Vector2 fadeDurationInOut = new(1.5f, 1.5f);

    [Space]
    [SerializeField] ParticleSystem particles;
    [SerializeField] WhiteFadeTransitioner whiteFadeTransitioner;
    [SerializeField] Vector2 emissionStartEnd;
    [Space]

    [Header("Velocity")]
    [SerializeField] Vector2 startSpeedStartEnd;
    [SerializeField] float startSpeedDiff;

    [Header("Size")]
    [SerializeField] Vector2 startSizeStartEnd;
    [SerializeField] float startSizeDiff;

    ParticleSystem.EmissionModule particlesEmissionMod;
    ParticleSystem.MainModule particlesMain;

    bool isPlayingParticles = false;

    protected override void Awake()
    {
        base.Awake();
        particlesEmissionMod = particles.emission;
        particlesMain = particles.main;
    }

    protected override float PerformTransition(float time, bool backwards = false)
    {
        base.PerformTransition(time, backwards);
        StopAllCoroutines();

        float fadeDelay = Utils.FromInOut(fadeDelayInOut, !backwards);
        float fadeDuration = Utils.FromInOut(fadeDurationInOut, !backwards);

        StartCoroutine(TransitionSequence(time, backwards, fadeDelay, fadeDuration));
        return fadeDelay + fadeDuration;
    }

    IEnumerator TransitionSequence(float time, bool backwards, float fadeDelay, float fadeDuration)
    {
        OnTransitionStart();

        // Play particles
        StartCoroutine(ParticlesSequence(time, backwards));

        // Wait for fade delay and play white fade transition
        yield return new WaitForSecondsRealtime(fadeDelay);
        whiteFadeTransitioner.Transitionate(fadeDuration, backwards);

        yield return new WaitUntil(() =>
            !isPlayingParticles && !whiteFadeTransitioner.IsTransitioning
            && particles.particleCount <= maxParticleCountForFinishing);

        OnTransitionFinish();
    }

    IEnumerator ParticlesSequence(float time, bool backwards)
    {
        isPlayingParticles = true;
        float elapsed = 0;

        particles.Play();

        // Interpolate values along the specified time
        while (elapsed < time)
        {
            float t = durationCurve.Evaluate(elapsed / time);
            if (backwards) t = 1 - t;

            particlesEmissionMod.rateOverTime = LerpMinMax(emissionStartEnd, t);
            particlesMain.startSpeed = LerpMinMaxCurve(startSpeedStartEnd, startSpeedDiff, t);
            particlesMain.startSize = LerpMinMaxCurve(startSizeStartEnd, startSizeDiff, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        isPlayingParticles = false;
        yield return new WaitForSecondsRealtime(particlesExtraTime);
        particles.Stop();
    }

    /// <summary>Returns a MinMaxCurve centered around a value interpolated between min and max, with a range defined by diff.</summary>
    /// <param name="minMax">Min and max values to interpolate between.</param>
    /// <param name="diff">Total range around the interpolated value.</param>
    /// <param name="t">Normalized value (0 to 1) for interpolation.</param>
    ParticleSystem.MinMaxCurve LerpMinMaxCurve(Vector2 minMax, float diff, float t)
    {
        float value = LerpMinMax(minMax, t);
        float halfDiff = diff / 2;
        return new(value - halfDiff, value + halfDiff);
    }

    /// <summary>Linearly interpolates between min and max using t (0 to 1).</summary>
    /// <param name="minMax">Min (x) and max (y) values.</param>
    /// <param name="t">Interpolation factor.</param>
    float LerpMinMax(Vector2 minMax, float t)
        => Mathf.Lerp(minMax.x, minMax.y, t);
}
