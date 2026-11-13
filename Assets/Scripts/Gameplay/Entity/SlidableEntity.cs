using System.Linq;
using UnityEngine;

public abstract class SlidableEntity : MotionEntity
{
    [field: Header("Slide Collision Checkers")]
    [field: SerializeField] protected Transform NextGroundChecker { get; private set; }
    [field: SerializeField] protected Transform ForwardChecker { get; private set; }
    [field: SerializeField] protected Transform EntityChecker { get; private set; }

    [field: Header("Slide Motion")]
    [field: SerializeField] public SlidableEntityConfig Config { get; private set; }

    [field: Header("Slide Particles")]
    [field: SerializeField] protected SlidingParticles SlidingParticles { get; private set; }

    Vector2 currentSlideDirection;
    Vector2 rotatingTowards = Vector2.zero;

    public bool IsSliding { get; private set; } = false;
    public bool IsStoppingToSlide { get; private set; } = false;
    public bool IsGonnaCollide { get; private set; } = false;

    bool isPlayingParticlesAndSound = false;
    SoundHandler slidingSoundLoop;

    protected override void Awake()
    {
        base.Awake();

        // Handle checkers
        Utils.PrefixAndUnParent(name, NextGroundChecker, ForwardChecker, EntityChecker);
        SetCheckersActive(false);
    }

    protected override void Update()
    {
        base.Update();
        SlidingProcess();
    }

    void SlidingProcess()
    {
        if (!IsSliding) return;
        float forwardCheckerDistance = Utils.Vectors.DistanceIgnoreY(transform.position, ForwardChecker.position);

        if (IsStoppingToSlide)
        {
            StopSlidingParticlesAndSFX();

            // Stop moving smoothly
            if (forwardCheckerDistance > Consts.MIN_MOVEMENT_DIST)
            {
                float adjustedSpeed = Config.Speed * Time.deltaTime * Mathf.Clamp01(forwardCheckerDistance);
                transform.position = Vector3.MoveTowards(transform.position, ForwardChecker.position, adjustedSpeed);
                return;
            }

            // Stop sliding
            ForceStopSliding();
            OnStopSliding();

            // Smoothly adjust position
            MoveTowards(ForwardChecker.position);
            return;
        }

        if (IsGonnaCollide || forwardCheckerDistance <= Config.NextPointMinDistance)
        {
            if (IsGonnaCollide)
            {
                OnCollide();
                currentSlideDirection = -currentSlideDirection;
                IsGonnaCollide = false;
                RotateTowardsDirection();
            }

            Vector3 newPathPoint = CreateNewPathPoint();
            RefreshCollisionCheckers(newPathPoint);
            return;
        }

        // Move towards next position
        float deltaSpeed = Config.Speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, ForwardChecker.position, deltaSpeed);
    }

    /// <summary>Requests the entity to start sliding and does so if possible.</summary>
    /// <returns>Wheter it started sliding or not.</returns>
    public bool RequestStartSliding(Vector2 direction)
    {
        Vector3 checkPosition = transform.position;
        checkPosition.y = -0.6f;
        bool isSteppingOnIce = PositionIsIce(checkPosition);

        if (isSteppingOnIce)
        {
            StartSliding(direction);
            return true;
        }
        return false;
    }

    private void StartSliding(Vector2 direction)
    {
        currentSlideDirection = direction;

        Vector3 newPathPoint = CreateNewPathPoint();
        SetCheckersActive(true);
        RefreshCollisionCheckers(newPathPoint);

        if (IsGonnaCollide)
        {
            ForceStopSliding();
            return;
        }

        IsSliding = true;
        RotateTowardsDirection();
        OnStartSliding();
    }

    /// <summary>Forces the entity to stop sliding. Does not call OnStopSliding.</summary>
    public void ForceStopSliding()
    {
        IsStoppingToSlide = false;
        IsSliding = false;
        IsGonnaCollide = false;

        SetCheckersActive(false);
    }

    Vector3 CreateNewPathPoint()
    {
        Vector3 currentPosition = Utils.Vectors.Round(transform.position);
        currentPosition.y = transform.position.y;
        Vector3 m_direction = new(currentSlideDirection.x, 0, currentSlideDirection.y);
        Vector3 newPathPoint = currentPosition + m_direction;

        OnNewPathPointCreated(newPathPoint);
        return newPathPoint;
    }

    void RefreshCollisionCheckers(Vector3 nextPosition)
    {
        // Set checkers positions
        ForwardChecker.position = nextPosition;
        NextGroundChecker.position = new(nextPosition.x, 0, nextPosition.z);

        EntityChecker.position = nextPosition;
        if (Config.StopOnHitEntity) EntityChecker.position += new Vector3(currentSlideDirection.x, 0, currentSlideDirection.y);

        Physics.SyncTransforms();

        // Check collisions
        bool nextGroundIsIce = PositionIsIce(NextGroundChecker.position);
        bool wouldCollideWithEntity = WouldCollideWithEntity();

        // New path point logic
        if (WouldCollideWithSolid() || (wouldCollideWithEntity && !Config.StopOnHitEntity))
        {
            IsGonnaCollide = true;
        }
        else if (wouldCollideWithEntity || !nextGroundIsIce)
        {
            IsStoppingToSlide = true;
            OnStartStoppingToSlide();
        }
    }

    bool PositionIsIce(Vector3 position)
        => Utils.IsColliding(position, LayerMasks.Ice.Mask);

    bool WouldCollideWithEntity()
    {
        return Utils.IsColliding(EntityChecker.position, Config.EntitiesMask)
            || Utils
                .GetCollidersAt(EntityChecker.position)
                .Any(collider =>
                {
                    MotionEntity entity = collider.GetComponent<MotionEntity>();
                    return entity != null && entity != this;
                });
    }

    bool WouldCollideWithSolid()
    {
        Collider[] colliders = Utils.GetCollidersAt(ForwardChecker.position);
        foreach (Collider collider in colliders)
        {
            if (Utils.IsInLayerMask(collider.gameObject.layer, Config.SolidsMask))
                return true;

            if (collider.GetComponent<Door>() is Door door)
                return !door.IsOpen;
        }
        return false;
    }

    void SetCheckersActive(bool value)
    {
        Transform[] checkers = { NextGroundChecker, ForwardChecker, EntityChecker };

        foreach (Transform checker in checkers)
            checker.gameObject.SetActive(value);
    }

    void SetPlayerCheckerColliderEnabled(bool value)
    {
        if (EntityChecker.gameObject.TryGetComponent(out BoxCollider playerCheckerCollider))
        {
            playerCheckerCollider.enabled = value;
        }
    }

    protected virtual void OnStartSliding()
    {
        SetCheckersActive(true);
        SetPlayerCheckerColliderEnabled(true);
        PlaySlidingParticlesAndSFX();
    }
    protected virtual void OnStopSliding() { }

    protected virtual void OnStartStoppingToSlide()
    {
        SetPlayerCheckerColliderEnabled(false);
    }

    protected virtual void OnNewPathPointCreated(Vector3 newPathPoint) { }

    protected virtual void OnCollide()
    {
        // Handle gong collision
        foreach (Collider collider in Utils.GetCollidersAt(ForwardChecker.position, LayerMasks.Wall.Mask))
        {
            if (collider.TryGetComponent(out Gong gong))
            {
                gong.Trigger();
                break;
            }
        }

        // Play wall hit SFX
        SFXPlayer
            .PlaySound(Config.WallHit)
            .SetVolume(0.9f)
            .SetPitchRange(0.1f, 0.5f)
            .SetFadeTime(0, 0.3f);

        // Spawn wall hit particles
        if (Config.WallHitParticlesObject != null)
        {
            Vector3 wallHitPosition = ForwardChecker.position;
            Vector3 hitDirection = new(currentSlideDirection.x, 0, currentSlideDirection.y);

            float distanceFromWall = Mathf.Clamp01(Config.WallHitParticlesWallDistance);
            Vector3 spawnPosition = wallHitPosition - (hitDirection * distanceFromWall);

            GameObject instantiatedObject =
                Instantiate(Config.WallHitParticlesObject, spawnPosition, Quaternion.identity);
            Destroy(instantiatedObject, 3f);
        }
    }

    void RotateTowardsDirection()
    {
        if (rotatingTowards != currentSlideDirection)
        {
            RotateModel(
                target: currentSlideDirection,
                time: Config.RotationTime,
                ease: Config.RotationEasing
            );
            rotatingTowards = currentSlideDirection;
        }
    }

    void PlaySlidingParticlesAndSFX()
    {
        if (isPlayingParticlesAndSound) return;

        SlidingParticles.Play();
        isPlayingParticlesAndSound = true;

        AudioClip clip = GameManager.I.StaticConfig.Audio.Clips.SlideOnIce;
        slidingSoundLoop = SFXPlayer
            .PlaySound(clip)?
            .SetVolume(0.85f)
            .SetFadeTime(0.3f, 0.7f)
            .EnableLooping();
    }

    void StopSlidingParticlesAndSFX()
    {
        if (!isPlayingParticlesAndSound) return;

        SlidingParticles.Stop();
        isPlayingParticlesAndSound = false;

        SFXPlayer.StopSound(slidingSoundLoop);
    }
}
