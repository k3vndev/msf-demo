using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>Handles the playback and settings of a sound effect.</summary>
/// <remarks>
/// This class allows for playing, pausing, stopping, fading, and customizing sound effects. It provides methods to set volume, pitch, fade times, looping, and easing.  
/// <br/><br/> Configuration methods (like <c>SetVolume</c>, <c>SetPitch</c>, etc.) won't take effect if called after <c>Play</c> has been invoked (one frame after the first call).
/// </remarks>
public class SoundHandler
{
    public readonly AudioSource AudioSource;
    readonly SFXPlayer SFXPlayer;
    Vector2 fadeTimeInOut = Vector2.one * 0.2f;
    float maxVolume = 1f;
    Action onFinish = null;
    Vector2 pitchRangeMinMax = new(-0.15f, 0.15f);

    /// <summary>Indicates if the sound is playing.</summary>
    public bool IsPlaying { get; private set; } = false;

    /// <summary>Indicates if the sound is paused.</summary>
    public bool IsPaused { get; private set; } = false;

    /// <summary>Indicates if the sound has finished playing and already faded out.</summary>
    public bool HasFinished { get; private set; } = false;

    /// <summary>Indicates if the sound is starting to play.</summary>
    public bool IsStarting { get; private set; } = true;

    /// <summary>Indicates if the sound is fading in.</summary>
    public bool IsFadingIn { get; private set; } = false;

    /// <summary>Indicates if the sound is fading out.</summary>
    public bool IsFadingOut { get; private set; } = false;

    Ease easeIn = Ease.InQuad;
    Ease easeOut = Ease.OutQuad;

    float playedTime = 0;
    float actualDuration;

    public SoundHandler(SFXPlayer sfxPlayer, AudioSource audioSource, AudioClip clip)
    {
        SFXPlayer = sfxPlayer;
        audioSource.clip = clip;
        AudioSource = audioSource;
    }

    /// <summary>Sets the maximum volume for the sound.</summary>
    /// <remarks>
    /// If the sound has fade times set, it will fade between 0 and the maximum volume.<br/>
    /// The volume is clamped between 0 and 1.<br/>
    /// Default value is 1.
    /// </remarks>
    public SoundHandler SetVolume(float volume)
    {
        maxVolume = Mathf.Clamp01(volume);
        return this;
    }

    /// <summary>Sets the symmetric range for the random pitch variation.</summary>
    /// <remarks>
    /// Default pitch range is <c>0.15f</c>, which means set value will be between -0.15 and 0.15.<br/>
    /// Pitch being zero means no change, negative values make the sound deeper and positive values make it higher.<br/><br/>
    /// Note: Pitch gets increased by 1 when setting to an AudioSource, since Unity's AudioSource pitch is 1 by default and represents the original pitch of the clip.
    /// </remarks>
    public SoundHandler SetPitchRange(float range)
    {
        range = Mathf.Abs(range);
        SetPitchRange(-range, range);
        return this;
    }

    /// <summary>Sets the asymmetric range for the random pitch variation.</summary>
    /// <remarks>
    /// Default pitch range is <c>0.15f</c>, which means set value will be between -0.15 and 0.15.<br/>
    /// Pitch being zero means no change, negative values make the sound deeper and positive values make it higher.<br/><br/>
    /// Note: Pitch gets increased by 1 when setting to an AudioSource, since Unity's AudioSource pitch is 1 by default and represents the original pitch of the clip.
    /// </remarks>
    public SoundHandler SetPitchRange(float min, float max)
    {
        pitchRangeMinMax.x = Mathf.Min(min, max);
        pitchRangeMinMax.y = Mathf.Max(min, max);
        return this;
    }

    /// <summary>Sets the pitch of the sound.</summary>
    /// <remarks>
    /// Default pitch range is <c>0.15f</c>, which means set value will be between -0.15 and 0.15.<br/>
    /// Pitch being zero means no change, negative values make the sound deeper and positive values make it higher.<br/><br/>
    /// Note: Pitch gets increased by 1 when setting to an AudioSource, since Unity's AudioSource pitch is 1 by default and represents the original pitch of the clip.
    /// </remarks>
    public SoundHandler SetPitch(float pitch)
    {
        pitchRangeMinMax = pitch * Vector2.one;
        return this;
    }

    /// <summary>Sets the fade time for both fade in and fade out.</summary>
    /// <remarks>Default fade time is 0.2 seconds for both fade in and fade out.</remarks>
    public SoundHandler SetFadeTime(float fadeTime)
    {
        SetFadeTime(fadeTime, fadeTime);
        return this;
    }

    /// <summary>Sets the fade times for fade in and fade out.</summary>
    /// <remarks>Default fade time is 0.2 seconds for both fade in and fade out.</remarks>
    public SoundHandler SetFadeTime(float fadeInTime, float fadeOutTime)
    {
        fadeTimeInOut.x = Mathf.Max(0f, fadeInTime);
        fadeTimeInOut.y = Mathf.Max(0f, fadeOutTime);
        return this;
    }

    /// <summary>Calls <c>onFinish</c> when the sound finishes playing.</summary>
    public SoundHandler OnFinish(Action onFinish)
    {
        this.onFinish = onFinish;
        return this;
    }

    /// <summary>Enables looping for the sound.</summary>
    /// <remarks>
    /// Looping sounds will loop forever until stopped externally. <br/>
    /// By default, looping is disabled.
    /// </remarks>
    public SoundHandler EnableLooping()
    {
        AudioSource.loop = true;
        return this;
    }

    /// <summary>Sets the easing for both fade in and fade out.</summary>
    /// <remarks>Default easing is InQuad for fade in and OutQuad fade out.</remarks>
    public SoundHandler SetEasing(Ease easing)
    {
        SetEasing(easing, easing);
        return this;
    }

    /// <summary>Sets the easing for fade in and fade out.</summary>
    /// <remarks>Default easing is InQuad for fade in and OutQuad fade out.</remarks>
    public SoundHandler SetEasing(Ease easeIn, Ease easeOut)
    {
        this.easeIn = easeIn;
        this.easeOut = easeOut;
        return this;
    }

    /// <summary>
    /// Plays the sound request and waits for the sound to finish.
    /// <list type="bullet">
    /// <item>If the sound is not looping, it will fade in and out based on the specified fade times.</item>
    /// <item>If the sound is looping, it will only fade in.</item>
    /// </list>
    /// </summary>
    /// <param name="dontStartUntil">Wait for the contition to evaluate <c>true</c> before playing the sound.</param>
    public IEnumerator Play(Func<bool> dontStartUntil = null)
    {
        // Wait a frame so the SoundHandler params can be set.
        yield return null;

        yield return new WaitUntil(() => dontStartUntil?.Invoke() ?? true);

        AudioSource.pitch = CalculatePitch();
        float totalFadeTime = fadeTimeInOut.x + fadeTimeInOut.y;
        actualDuration = Mathf.Abs(AudioSource.clip.length / AudioSource.pitch);

        AudioSource.Play();
        IsStarting = false;
        IsPlaying = true;

        // Track playing time
        SFXPlayer.StartCoroutine(TrackPlayingTime());

        if (AudioSource.loop)
        {
            // If the sound is looping, just fade in
            FadeIn();

            yield return new WaitUntil(() => HasFinished);
            yield break;
        }

        // For non looping sounds
        if (actualDuration > totalFadeTime && totalFadeTime > 0f)
        {
            // If the sound is not looping, fade in and out
            FadeIn();

            yield return new WaitUntil(() =>
                playedTime >= actualDuration - fadeTimeInOut.y || HasFinished);

            if (HasFinished) yield break;

            yield return FadeOutAndStop();
            yield break;
        }
        else
        {
            // If the sound is not looping or too short, just play it at max volume
            AudioSource.volume = maxVolume;
        }

        // Wait for the sound to fully stop and if the sound is not playing anymore, stop the coroutine
        yield return new WaitUntil(() =>
            playedTime >= actualDuration || IsFadingOut || HasFinished);

        if (IsFadingOut || HasFinished) yield break;

        // This point must ONLY be reached for sounds that arent looping or stopped early
        Stop();
    }

    /// <summary>Plays the sound and waits for the fade out to complete.</summary>
    public IEnumerator FadeOutAndStop()
    {
        if (!IsPaused && !HasFinished && !IsFadingOut)
        {
            FadeOut();
            yield return new WaitForSecondsRealtime(fadeTimeInOut.y);
        }
        Stop();
    }

    /// <summary>Stops the sound immediately.</summary> 
    public void Stop()
    {
        if (HasFinished) return;

        HasFinished = true;
        IsPlaying = false;

        onFinish?.Invoke();

        AudioSource.Stop();
        AudioSource.DOKill();
    }

    public void Pause()
    {
        if (IsPaused || !IsPlaying) return;

        IsPaused = true;
        DOTween.Pause(AudioSource);
        AudioSource.Pause();
    }

    public void Resume()
    {
        if (!IsPaused || !IsPlaying) return;

        IsPaused = false;
        DOTween.Play(AudioSource);
        AudioSource.UnPause();
    }

    void FadeIn()
    {
        if (IsFadingIn) return;

        IsFadingIn = true;
        AudioSource.volume = 0f;
        Fade(maxVolume, fadeTimeInOut.x, easeIn, () => IsFadingIn = false);
    }

    void FadeOut()
    {
        if (IsFadingOut) return;

        IsFadingOut = true;
        Fade(0f, fadeTimeInOut.y, easeOut, () => IsFadingOut = false);
    }

    void Fade(float targetVolume, float fadeTime,
        Ease easing, Action onComplete = null)
    {
        AudioSource.DOKill();

        if (fadeTime <= 0f)
        {
            AudioSource.volume = targetVolume;
            return;
        }

        AudioSource
            .DOFade(targetVolume, fadeTime)
            .SetEase(easing)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(() => onComplete?.Invoke());
    }

    float CalculatePitch()
    {
        float value = UnityEngine.Random.Range(pitchRangeMinMax.x, pitchRangeMinMax.y) + 1f;
        return Mathf.Clamp(value, 0.01f, 2.5f);
    }

    IEnumerator TrackPlayingTime()
    {
        playedTime = 0f;
        while (IsPlaying)
        {
            if (!IsPaused) playedTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}