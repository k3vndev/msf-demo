using DG.Tweening;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager I { get; private set; }
    public SoundHandler CurrentMusic { get; private set; } = null;
    public SoundHandler CurrentAmbience { get; private set; } = null;

    [field: Header("Music")]
    [field: SerializeField, Range(0f, 1f)] public float MusicDefaultVolume { get; private set; } = 0.55f;
    [SerializeField] Ease musicEasing = Ease.OutQuint;
    [SerializeField] SFXPlayer musicPlayer;

    [field: Header("Ambience")]
    [field: SerializeField, Range(0f, 1f)] public float AmbienceDefaultVolume { get; private set; } = 0.4f;
    [SerializeField] AudioClip ambienceClip;
    [SerializeField] SFXPlayer ambiencePlayer;

    public enum FadeOutTime
    {
        Quick,
        Normal,
        Slow
    }

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    public void PlayMusic(AudioClip clip)
    {
        StopMusic();

        CurrentMusic = musicPlayer
            .PlaySound(clip)
            .SetVolume(MusicDefaultVolume)
            .SetPitch(0f)
            .SetFadeTime(0f)
            .SetEasing(Ease.Linear, musicEasing)
            .EnableLooping();
    }

    public void StopMusic(FadeOutTime fadeOutTime)
    {
        StopAmbience();
        if (CurrentMusic == null) return;

        float time = fadeOutTime switch
        {
            FadeOutTime.Quick => 1.2f,
            FadeOutTime.Normal => 2.5f,
            FadeOutTime.Slow => 7.5f,
            _ => 2f
        };

        CurrentMusic.SetFadeTime(0, time);
        StartCoroutine(CurrentMusic.FadeOutAndStop());
        CurrentMusic = null;
    }

    public void StopMusic() => StopMusic(FadeOutTime.Normal);

    public void PlayAmbience()
    {
        CurrentAmbience ??= ambiencePlayer
            .PlaySound(ambienceClip)
            .SetVolume(AmbienceDefaultVolume)
            .SetPitch(0f)
            .SetFadeTime(1.5f)
            .SetEasing(Ease.Linear)
            .EnableLooping();
    }

    public void StopAmbience()
    {
        if (CurrentAmbience == null) return;

        StartCoroutine(CurrentAmbience.FadeOutAndStop());
        CurrentAmbience = null;
    }
}
