using System.Collections;
using UnityEngine;

public abstract class CatAnimations : MonoBehaviour
{
    [SerializeField] ParticleSystem sleepingParticles;

    [Header("Idle Animations")]
    [SerializeField] Vector2 idleStandUpStartMinMax;
    [SerializeField] Vector2 idleStandUpLengthMinMax;
    [SerializeField] Vector2 idleLookAroundMinMax;
    [SerializeField] Vector2 idleSleepMinMax;
    [SerializeField, Range(0, 1)] float idleStandUpChance;
    [SerializeField, Range(0, 1)] float idleLookAroundChance;
    protected Animator animator;
    protected bool idle = false;
    Coroutine idleProcessCoroutine;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start() { }
    protected virtual void Update() { }

    IEnumerator IdleProcess()
    {
        idle = true;

        // Randomly play or not the idle stand up animation based on chance
        float standUpChance = Random.Range(0f, 1f);
        if (standUpChance < idleStandUpChance)
        {
            yield return new WaitForSeconds(RandomTime(idleStandUpStartMinMax));
            StartGreeting();
            yield return new WaitForSeconds(RandomTime(idleStandUpLengthMinMax));
            StopGreeting();
        }

        // Randomly play look around animation based on chance
        float lookAroundChance = Random.Range(0f, 1f);
        if (lookAroundChance < idleLookAroundChance)
        {
            yield return new WaitForSeconds(RandomTime(idleLookAroundMinMax));
            animator.SetTrigger(ANIM_NAMES.LookAround);
        }

        // Play sleeping animation after a delay
        yield return new WaitForSeconds(RandomTime(idleSleepMinMax));
        StartSleeping();

        // Wait for the sleeping animation to actually start before playing the particles
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Sleeping"));
        sleepingParticles.Clear();
        sleepingParticles.Play();
    }

    protected void StartIdleProcess()
    {
        if (idle) return;
        idleProcessCoroutine = StartCoroutine(IdleProcess());
    }

    protected void StopIdleProcess()
    {
        if (!idle) return;
        idle = false;

        if (idleProcessCoroutine != null)
        {
            StopCoroutine(idleProcessCoroutine);
            idleProcessCoroutine = null;
        }

        StopSleeping();
        StopGreeting();
    }

    protected void StartSleeping()
    {
        animator.SetTrigger(ANIM_NAMES.Sleep);
        animator.SetBool(ANIM_NAMES.IsSleeping, true);
    }

    protected void StopSleeping()
    {
        animator.SetBool(ANIM_NAMES.IsSleeping, false);
        sleepingParticles.Stop();
    }

    protected void StartGreeting()
    {
        animator.SetTrigger(ANIM_NAMES.Greet);
        animator.SetBool(ANIM_NAMES.IsGreeting, true);
    }

    protected void StopGreeting()
    {
        animator.SetBool(ANIM_NAMES.IsGreeting, false);
    }

    protected void StartHeating()
    {
        animator.SetTrigger(ANIM_NAMES.Heat);
        animator.SetBool(ANIM_NAMES.IsHeating, true);
    }

    protected void StopHeating()
    {
        animator.SetBool(ANIM_NAMES.IsHeating, false);
    }

    protected void StartFreezing()
    {
        animator.SetTrigger(ANIM_NAMES.Freeze);
        animator.SetBool(ANIM_NAMES.IsFreezing, true);
    }

    protected void StopFreezing()
    {
        animator.SetBool(ANIM_NAMES.IsFreezing, false);
    }

    protected float RandomTime(Vector2 timeRange)
        => Random.Range(timeRange.x, timeRange.y);


    protected static class ANIM_NAMES
    {
        public const string IsDancing = "is-dancing";
        public const string LookAround = "look-around";
        public const string IsGreeting = "is-greeting";
        public const string Greet = "greet";
        public const string IsSleeping = "is-sleeping";
        public const string Sleep = "sleep";

        public const string IsHeating = "is-heating";
        public const string Heat = "heat";

        public const string IsFreezing = "is-freezing";
        public const string Freeze = "freeze";
    }
}