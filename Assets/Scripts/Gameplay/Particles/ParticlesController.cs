using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    public bool IsPlaying { get; private set; } = false;

    public virtual void Play()
    {
        IsPlaying = true;
    }

    public virtual void Stop()
    {
        IsPlaying = false;
    }
}