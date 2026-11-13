using UnityEngine;

public class HitGroundParticles : ParticlesController
{
    [SerializeField] ParticleSystem snowParticles;
    [SerializeField] ParticleSystem dustParticles;

    public override void Play()
    {
        base.Play();
        snowParticles.Clear();
        dustParticles.Clear();

        snowParticles.Play();
        dustParticles.Play();
    }

    public override void Stop()
    {
        base.Stop();
        snowParticles.Stop();
        dustParticles.Stop();
    }

}