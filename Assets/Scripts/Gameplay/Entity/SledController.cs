using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;
using System.Collections.Generic;

public class SledController : SlidableEntity
{
    [Header("Motion")]
    [SerializeField] float adjustTime = 0.17f;
    [SerializeField] float playerAdjustRotationTime = 0.29f;
    [SerializeField] Ease adjustEase = Ease.OutCubic;

    /// <summary>Indicates whether the sled is currently being carried by the player.</summary>
    public bool IsBeingCarried { get; private set; } = false;
    public bool IsStoppingToBeCarried { get; private set; } = false;
    public bool IsStartingToBeCarried { get; private set; } = false;
    public bool IsAdjustingPlayerRotation { get; private set; } = false;
    public bool IsAdjustingPosition { get; private set; } = false;
    public bool IsAdjustingRotation { get; private set; } = false;

    public static event Action OnCarryStart;

    [Space]
    [SerializeField] float startCarryingCooldown;
    float startCarryingCooldownRemaining = 0;

    PlayerController playerController;
    PlayerMovement playerMovement;
    PlayerInput playerInput;
    Coroutine adjustRotationCoroutine;

    protected override void Awake()
    {
        base.Awake();

        playerController = Utils.GetComp<PlayerController>("Player");
        playerMovement = playerController.GetComponent<PlayerMovement>();
        playerInput = playerController.GetComponent<PlayerInput>();
    }

    protected override void Update()
    {
        base.Update();

        // Handle carrying cooldown timer
        if (startCarryingCooldownRemaining > 0)
            startCarryingCooldownRemaining -= Time.deltaTime;

        // Stop carrying if the player is not in range
        if (IsBeingCarried && !playerController.SledIsOnRange
            && !playerMovement.IsRotating && playerController.CanInteract)
        {
            StopCarrying();
        }
    }

    public void StartCarrying()
    {
        if (!IsBeingCarried && !IsStartingToBeCarried
            && !IsStoppingToBeCarried && startCarryingCooldownRemaining <= 0
            && (!IsSliding || IsStoppingToSlide))
        {
            StartCoroutine(StartCarryingSequence());
        }
    }

    IEnumerator StartCarryingSequence()
    {
        IsStartingToBeCarried = true;

        playerMovement.DisableMovement();
        playerMovement.SnapPosition();

        if (IsStoppingToSlide)
        {
            ForceStopSliding();
        }

        // Adjusts player rotation so its facing the sled
        IsAdjustingPlayerRotation = true;
        Vector2 playerAdjustRotation = GetDirectionFromPlayer();

        playerMovement.RotateModel(
           target: playerAdjustRotation,
           time: playerAdjustRotationTime,
           ease: Ease.InOutCubic,
           onFinish: () => IsAdjustingPlayerRotation = false
       );

        // Wait until player stops moving
        yield return new WaitUntil(() =>
            !playerMovement.IsMoving || !playerInput.ActionKeyIsPressed);

        // Don't proceed if ActionKey is not being pressed
        if (!playerInput.ActionKeyIsPressed)
        {
            playerMovement.EnableMovement();
            IsStartingToBeCarried = false;
            yield break;
        }

        // Adjusts sled position to be on the player
        GridSnapToPlayer();

        // Wait until player and sled stop adjusting
        yield return new WaitUntil(() =>
            (!IsAdjustingPlayerRotation && !IsAdjustingPosition
            && !IsAdjustingRotation && !playerMovement.IsRotating)
            || !playerInput.ActionKeyIsPressed);

        IsAdjustingPlayerRotation = false;
        IsAdjustingPosition = false;
        IsAdjustingRotation = false;

        // Don't proceed if ActionKey is not being pressed
        if (!playerInput.ActionKeyIsPressed)
        {
            playerMovement.EnableMovement();
            IsStartingToBeCarried = false;
            yield break;
        }

        OnCarryStart?.Invoke();

        ForceStartCarrying();
        playerMovement.EnableMovement();
    }

    /// <summary>Forces the player to start carrying the sled without any check or secondary process.</summary>
    public void ForceStartCarrying(bool supportsRotation = false)
    {
        // Initially set the parent as the player, not its mesh model
        Transform newParent = supportsRotation
            ? playerMovement.MeshModel
            : playerMovement.transform;

        transform.SetParent(newParent);
        SetLayer(LayerMasks.Player.Index);

        IsBeingCarried = true;
        IsStartingToBeCarried = false;
    }

    /// <summary>Makes the player stop carrying the sled with some checks and position fixing.</summary>
    public void StopCarrying()
    {
        if (!IsStoppingToBeCarried && !IsStartingToBeCarried && IsBeingCarried)
            StartCoroutine(StopCarryingSequence());
    }

    /// <summary>Forces the player to stop carrying the sled without any check or secondary process.</summary>
    public void ForceStopCarrying()
    {
        IsBeingCarried = false;
        transform.SetParent(null);
        SetLayer(LayerMasks.Sled.Index);
    }

    IEnumerator StopCarryingSequence()
    {
        IsStoppingToBeCarried = true;

        playerMovement.DisableMovement();
        playerMovement.SnapPosition();

        yield return new WaitUntil(() => !playerMovement.IsMoving);

        playerMovement.EnableMovement();

        IsBeingCarried = false;
        transform.SetParent(null);

        SetLayer(LayerMasks.Sled.Index);
        GridSnapToWorld();

        // Wait until it stops adjusting
        yield return new WaitUntil(() =>
            !IsAdjustingPosition && !IsAdjustingRotation);

        startCarryingCooldownRemaining = startCarryingCooldown;
        IsStoppingToBeCarried = false;

        RequestStartSliding(GetDirectionFromPlayer());
    }

    public override void RotateModel(
        Vector2 target, float time, Ease? ease = null,
        Action onFinish = null)
    {
        float targetDeg = Utils.Vectors.VecToDeg(target);
        Vector3 euler = MeshModel.rotation.eulerAngles;
        Quaternion m_targetRotation = Quaternion.Euler(euler.x, targetDeg - 90, euler.z);

        if (Quaternion.Angle(m_targetRotation, MeshModel.rotation) < Consts.MIN_ROTATION_DEG)
        {
            onFinish?.Invoke();
            return;
        }

        IsAdjustingRotation = true;
        DOTween.Kill(TweenIds.SledRotate);
        Ease easing = ease ?? adjustEase;

        MeshModel
            .DORotateQuaternion(m_targetRotation, time)
            .SetId(TweenIds.SledRotate)
            .SetEase(easing)
            .OnComplete(() =>
            {
                IsAdjustingRotation = false;
                onFinish?.Invoke();
            });
    }

    public override void SnapPosition()
    {
        Vector3 targetPosition = Utils.Vectors.Round(transform.position);
        MoveTowards(targetPosition);
    }

    public override void MoveTowards(Vector3 targetPosition, bool rotateModel = false)
    {
        MoveTowards(targetPosition, adjustTime, adjustEase, rotateModel);
    }

    public void MoveTowards(Vector3 targetPosition, float time, Ease easing, bool rotateModel = false)
    {
        Vector3 m_targetPosition = targetPosition;
        m_targetPosition.y = transform.position.y;

        // Snap position. If its too close already, snap instantly 
        if (Vector3.Distance(transform.position, m_targetPosition) >= Consts.MIN_MOVEMENT_DIST)
        {
            IsAdjustingPosition = true;
            DOTween.Kill(TweenIds.SledMove);

            transform
                .DOMove(m_targetPosition, time)
                .SetId(TweenIds.SledMove)
                .SetEase(easing)
                .OnComplete(() => IsAdjustingPosition = false);
        }
        else transform.position = m_targetPosition;
    }

    /// <summary>Takes the sled as a child of the player and snaps its local position.</summary>
    public void GridSnapToPlayer()
    {
        Vector2 directionFromPlayer = GetDirectionFromPlayer();
        Vector3 snappedPosition = playerMovement.transform.position
            + new Vector3(directionFromPlayer.x, 0, directionFromPlayer.y);

        MoveTowards(snappedPosition);
        RotateModel(directionFromPlayer, adjustTime);
    }

    /// <summary>Snaps its global position.</summary>
    public void GridSnapToWorld()
    {
        float targetDeg = Mathf.Round(Mathf.Repeat(MeshModel.eulerAngles.y, 360) / 90) * 90;
        Vector2 targetRotation = Utils.Vectors.DegToVec(targetDeg + 90);

        SnapPosition();
        RotateModel(targetRotation, adjustTime);
    }

    void OnPlayerRotationStarted()
    {
        if (IsBeingCarried && transform.parent != playerMovement.MeshModel)
        {
            // Adjust the sled's  position to match the player's rotation
            Vector3 playerMeshright = playerMovement.MeshModel.right;
            playerMeshright.y = transform.localPosition.y;
            transform.localPosition = playerMeshright;

            // Set the parent as the player mesh model
            ForceStartCarrying(supportsRotation: true);

            // Start adjusting the sled's rotation to match the player's rotation
            if (adjustRotationCoroutine != null) StopCoroutine(adjustRotationCoroutine);
            adjustRotationCoroutine = StartCoroutine(AdjustRotationWhileCarrying());
        }
    }

    /// <summary>Continuously tries to adjust the sled's rotation to match the player's rotation until successful.</summary>
    IEnumerator AdjustRotationWhileCarrying()
    {
        while (true)
        {
            // First, wait until the player stops rotating
            bool rotationFinished = false;
            yield return new WaitUntil(() => !playerMovement.IsRotating);

            // Try to adjust the sled's rotation to match the player's rotation
            float playerRotation = playerMovement.MeshModel.eulerAngles.y;
            Vector2 targetRotation = Utils.Vectors.DegToVec(playerRotation + 90f);
            RotateModel(targetRotation, adjustTime, onFinish: () => rotationFinished = true);

            yield return new WaitUntil(() =>
                rotationFinished || playerMovement.IsRotating || !IsBeingCarried);

            // If the rotation is finished or the sled is no longer being carried, exit the loop
            if (rotationFinished || !IsBeingCarried) yield break;

            // Kill any previous rotation tween before continuing the loop
            DOTween.Kill(TweenIds.SledRotate);
        }
    }

    public Vector2 GetDirectionFromPlayer()
        => Utils.Vectors.ToDirectional(Utils.Vectors.CalcDirection(playerMovement.transform.position, transform.position));

    void SetLayer(int layer) => gameObject.layer = layer;

    protected override void OnCurrentCollidersRefresh()
    {
        base.OnCurrentCollidersRefresh();
        PressableChecker pressableChecker = new(this, CurrentColliders, PreviousColliders);
        pressableChecker.Execute();
    }

    void OnEnable()
    {
        PlayerMovement.OnRotationStarted += OnPlayerRotationStarted;
    }

    void OnDisable()
    {
        PlayerMovement.OnRotationStarted -= OnPlayerRotationStarted;

        DOTween.Kill(TweenIds.SledMove);
        DOTween.Kill(TweenIds.SledRotate);
    }

    enum TweenIds
    {
        SledMove,
        SledRotate,
    }
}
