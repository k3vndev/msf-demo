using UnityEngine;

public class SlidingParticles : ParticlesController
{
    [SerializeField] ParticleSystem[] particles;

    public override void Play()
    {
        base.Play();
        foreach (ParticleSystem particle in particles)
        {
            particle.Clear();
            particle.Play();
        }
    }

    public override void Stop()
    {
        base.Stop();
        foreach (ParticleSystem particle in particles)
            particle.Stop();
    }
}