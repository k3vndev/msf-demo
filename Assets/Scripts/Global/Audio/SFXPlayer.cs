using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField, Tooltip("If true, all sounds will be paused when the game is paused.")]
    bool pauseOnGamePause = true;

    [SerializeField, Range(1, 12), Tooltip("The number of audio channels to use. " +
        "The more channels, the more sounds can be played at the same time.")]
    int channelsPoolSize = 3;

    [SerializeField, Tooltip("The audio mixer group to route the audio through.")]
    AudioOutputGroup outputGroup = AudioOutputGroup.SFX;
    enum AudioOutputGroup { SFX, Music }

    [SerializeField, Tooltip("The spatial blend of the audio.")]
    SpatialBlend spatialBlend = SpatialBlend._3D;
    enum SpatialBlend { _2D, _3D }

    readonly Stack<AudioSource> availableChannelsPool = new();
    readonly List<SoundHandler> activeSounds = new();


    void Awake()
    {
        InitializePool();
    }

    /// <summary>
    /// Plays the given <c>AudioClip</c> using an available audio channel, if one exists.
    /// </summary>
    /// <param name="audioClip">The audio clip to play.</param>
    /// <returns>
    /// A <c>SoundHandler</c> instance for chaining settings or stopping the sound manually,
    /// or <c>null</c> if all audio channels are in use.
    /// </returns>
    /// <remarks>
    /// If no channels are available, nothing is played. Use optional chaining or null checks to avoid issues.
    /// </remarks>
    public SoundHandler PlaySound(AudioClip audioClip)
    {
        // Get an available audio channel
        AudioSource audioSource = GetAvailableChannel();
        if (audioSource == null) return null;

        // Create a new sound handler and start its lifetime coroutine
        SoundHandler soundHandler = new(this, audioSource, audioClip);
        StartCoroutine(HandleSoundLifetime(soundHandler));
        return soundHandler;
    }

    /// <summary>Handles the lifetime of a sound handler.</summary>
    /// <remarks>
    /// It adds the sound handler to the active sounds list,
    /// starts its lifetime coroutine and removes it from the list when it finishes.
    /// </remarks>
    IEnumerator HandleSoundLifetime(SoundHandler soundHandler)
    {
        activeSounds.Add(soundHandler);

        yield return soundHandler.Play(dontStartUntil: () =>
            !pauseOnGamePause || ((PauseMenuManager.I?.IsPaused ?? false) == false)
        );
        yield return new WaitUntil(() => soundHandler.HasFinished);
        RemoveSound(soundHandler);
    }

    /// <summary>Stops all currently playing sounds, fading them out if necessary.</summary>
    public void StopAllSounds()
    {
        foreach (var sound in activeSounds)
            StopSound(sound);
    }

    /// <summary>Stops all currently looping sounds, fading them out if necessary.</summary>
    public void StopAllLoopingSounds()
    {
        List<SoundHandler> loopingSounds = activeSounds
            .FindAll(sound => sound.AudioSource.loop);

        foreach (var sound in loopingSounds)
            StopSound(sound);
    }

    /// <summary>Stops a specific sound handler, fading it out if necessary.</summary>
    /// <remarks>It also checks if the sound handler is not null before attempting to stop it.</remarks>
    public void StopSound(SoundHandler soundHandler)
    {
        if (soundHandler != null)
            StartCoroutine(soundHandler.FadeOutAndStop());
    }

    /// <summary>Gets an available audio channel from the pool.</summary>
    /// <remarks>If no channels are available, a warning is displayed and <c>null</c> is returned.</remarks>
    /// <returns>An <c>AudioSource</c> instance if one is available, otherwise <c>null</c>.</returns>
    AudioSource GetAvailableChannel()
    {
        GarbageCollectSounds();

        // Check if theres and available channel and return it
        if (availableChannelsPool.Count > 0)
            return availableChannelsPool.Pop();

        // Otherwise, display a warning and return null
        LogWarning($"SFX pool exhausted! ({activeSounds.Count} channels in use). Consider increasing channelsPoolSize.");

        return null;
    }

    /// <summary>Removes a sound handler from the active sounds list and returns the audio source to the pool.</summary>
    void RemoveSound(SoundHandler soundHandler)
    {
        AudioSource source = soundHandler?.AudioSource;

        // Remove the sound handler from the active sounds list
        bool removed = activeSounds.Remove(soundHandler);

        if (!removed)
        {
            // If the sound handler was not found, display a warning
            string clipName = source.clip?.name ?? "null";
            LogWarning($"Tried to remove sound {clipName} but it was not found in the active sounds list.");
            return;
        }

        // Reset the audio source and return it to the pool
        ResetAudioSource(source);
        availableChannelsPool.Push(source);
    }

    /// <summary>Initializes the pool of audio sources.</summary>
    void InitializePool()
    {
        for (int i = 0; i < channelsPoolSize; i++)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            ResetAudioSource(audioSource);
            availableChannelsPool.Push(audioSource);
        }
    }

    /// <summary>Resets the audio source to its default state.</summary>
    void ResetAudioSource(AudioSource audioSource)
    {
        audioSource.playOnAwake = false;
        audioSource.clip = null;
        audioSource.pitch = 1f;
        audioSource.volume = 0;
        audioSource.loop = false;
        audioSource.spatialBlend = (int)spatialBlend;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.maxDistance = 19f;

        audioSource.outputAudioMixerGroup = outputGroup switch
        {
            AudioOutputGroup.SFX => GameManager.I.StaticConfig.Audio.Groups.SFX,
            AudioOutputGroup.Music => GameManager.I.StaticConfig.Audio.Groups.Music,
            _ => null
        };
    }

    /// <summary>Garbage collects the active sounds list.</summary>
    void GarbageCollectSounds()
    {
        List<SoundHandler> toRemove = activeSounds
            .FindAll(sound => sound.HasFinished);

        foreach (SoundHandler sound in toRemove)
        {
            RemoveSound(sound);
            string clipName = sound?.AudioSource?.clip?.name ?? "null";
            LogWarning($"Garbage Collector removed sound {clipName} because it had finished already.");
        }
    }

    void HandleTogglePause(bool value)
    {
        if (!pauseOnGamePause) return;

        foreach (SoundHandler sound in activeSounds)
        {
            if (value) sound.Pause();
            else sound.Resume();
        }
    }

    void LogWarning(string message)
    {
        Debug.LogWarning($"SFXPlayer: {message} " +
            $"At SFXPlayer: {name} in {transform.parent?.name ?? "Scene Root"}.");
    }

    void OnEnable() => PauseMenuManager.OnTogglePause += HandleTogglePause;
    void OnDisable() => PauseMenuManager.OnTogglePause -= HandleTogglePause;
}