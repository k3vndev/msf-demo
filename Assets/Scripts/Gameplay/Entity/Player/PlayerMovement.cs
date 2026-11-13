using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MotionEntity
{
    [Header("Configuration")]
    [field: Space, SerializeField] public PlayerMovementConfig Config { get; private set; }

    /// <summary>Wether if the player is currently moving towards the next path point.</summary>
    public bool IsMoving { get; private set; } = false;

    /// <summary>Whether the player is rotating the model.</summary>
    /// <remarks>Turns false early when close enough to the target, even if the tween isn't fully done.</remarks>
    public bool IsRotating { get; private set; } = false;

    /// <summary>Wether if the player is actually rotating the model, meaning that the rotation tween is in progress.</summary>
    public bool IsActuallyRotating { get; private set; } = false;

    /// <summary>Wether if the movement process is considering the user input or not.</summary>
    public bool IsUsingInput { get; private set; } = true;

    /// <summary>Wether if the player is currently moving towards the next path point.</summary>
    public bool IsGoingBackwards { get; private set; } = false;

    public Vector2 Velocity { get; private set; } = Vector2.zero;
    public float AxisVelocity { get; private set; } = 0;

    /// <summary> Invoked right before a new path point is created. </summary>
    public static event Action OnNewPathPoint;

    /// <summary> Invoked when the player stops moving. </summary>
    public static event Action OnPathCompleted;

    /// <summary> Invoked when a move attempt is cancelled because of collisions. </summary>
    public static event Action OnNewPathPointCancelled;

    /// <summary> Invoked when model rotation starts. </summary>
    public static event Action OnRotationStarted;

    /// <summary> Invoked only when model rotation reaches its target. </summary>
    public static event Action OnRotationCompleted;

    /// <summary> Invoked when model rotation finishes, even if it was interrupted. </summary>
    public static event Action OnRotationFinished;

    /// <summary> Invoked when a rotation attempt is cancelled because of collisions. </summary>
    /// <remarks> Only applies when the player is carrying the sled. </remarks>
    public static event Action OnCarryingRotationCancelled;

    [Header("Snap Transforms")]
    [SerializeField] Transform towardsSnap;
    [SerializeField] Transform currentSnap;
    [SerializeField] bool showSnapGizmos;

    Vector2 previousPosition;
    Quaternion rotatingTowards, previousRotation;
    Vector2 modelFacingDirection = new(1, 0);
    Axis movementAxis;
    Vector3 smoothMovementCurrentVelocity;
    SledController sledController;
    PlayerInput input;
    PlayerCollisions collisions;
    Coroutine rotationCoroutine;

    protected override void Awake()
    {
        base.Awake();

        input = GetComponent<PlayerInput>();
        collisions = GetComponent<PlayerCollisions>();

        rotatingTowards = MeshModel.localRotation;
        previousRotation = transform.rotation;
        previousPosition = Utils.Vectors.IgnoreY(transform.position);

        // Initialize snapTransforms
        Utils.PrefixAndUnParent("Player", towardsSnap, currentSnap);
    }

    protected override void Start()
    {
        base.Start();
        sledController = Utils.GetComp<SledController>("Sled");
    }

    protected override void Update()
    {
        base.Update();
        MovementProcess();

        CalcVelocity();
    }

    void MovementProcess()
    {
        float towardsDistance = Utils.Vectors
            .DistanceIgnoreY(transform.position, towardsSnap.position);

        if (IsMoving)
        {
            if (towardsDistance > Consts.MIN_MOVEMENT_DIST)
            {
                // Move player towards the next snap position
                Vector3 target = towardsSnap.position;
                target.y = transform.position.y;

                transform.position = Vector3.SmoothDamp(
                    current: transform.position,
                    target: target,
                    currentVelocity: ref smoothMovementCurrentVelocity,
                    smoothTime: Config.AccelerationTime,
                    maxSpeed: Config.Speed
                );
            }
            else
            {
                // Snap player on the towardsSnap position and stop movement
                Vector3 newPosition = towardsSnap.position;
                newPosition.y = transform.position.y;
                transform.position = newPosition;

                IsMoving = false;
                IsGoingBackwards = false;
                OnPathCompleted?.Invoke();
            }

            if (IsUsingInput)
            {
                // Move player back if they haven't moved much and are not moving along the axis
                bool IsNotMovingOnItsAxis = (movementAxis == Axis.Horizontal && input.Movement.x == 0)
                    || (movementAxis == Axis.Vertical && input.Movement.y == 0);

                if (towardsDistance > Config.TracePathReturnThreshold && IsNotMovingOnItsAxis)
                {
                    ResetPosition();
                }
            }
        }

        // Handle path tracing
        if (IsUsingInput && towardsDistance < Config.TracePathThreshold && input.Movement != Vector2.zero)
        {
            if (sledController.IsBeingCarried) CarryingPathTracing();
            else SoloPathTracing();
        }
    }

    /// <summary>Movement path tracing for when the player IS NOT carrying the sled.</summary>
    void SoloPathTracing()
    {
        CreateNewPathPoint();
        RotateModel(input.Movement, Config.SoloRotationTime);

        collisions.RefreshMoveChecker();

        // Reset path point if it's colliding towards
        if (collisions.WouldCollideAtMove() || collisions.WouldStepOnIce())
        {
            ResetPosition();
            OnNewPathPointCancelled?.Invoke();
            return;
        }
        IsMoving = true;
    }

    /// <summary>Movement path tracing for when the player IS carrying the sled.</summary>
    void CarryingPathTracing()
    {
        Transform moveCollisionChecker = collisions.RefreshMoveChecker();

        Vector2 sledDirection = sledController.GetDirectionFromPlayer();
        Axis sledAxis = sledDirection.x != 0 ? Axis.Horizontal : Axis.Vertical;

        if (sledAxis == input.Axis)
        {
            Vector3 addedPosition = CreateNewPathPoint();

            IsGoingBackwards = input.Movement.x + input.Movement.y
                != modelFacingDirection.x + modelFacingDirection.y;

            // Move collider further away if player is moving towards the sled
            if (!IsGoingBackwards)
                moveCollisionChecker.position += addedPosition;

            if (collisions.WouldCollideAtMove() || collisions.WouldStepOnIce())
            {
                ResetPosition();
                OnNewPathPointCancelled?.Invoke();
                return;
            }
            IsMoving = true;
        }
        else if (!IsRotating)
        {
            // Set RotateCollisionChecker position
            Transform rotateCollisionChecker = collisions.RefreshRotateChecker();
            Vector3 toAddPosition = new(modelFacingDirection.x, 0, modelFacingDirection.y);

            rotateCollisionChecker.position += toAddPosition;

            // Don't rotate if it's gonna collide
            bool wouldCollide = collisions.WouldCollideAtMove()
                || collisions.WouldCollideAtRotate();

            if (!wouldCollide)
            {
                DisableMovement();
                RotateModel(
                    target: input.Movement,
                    time: Config.CarryingRotationTime,
                    ease: Ease.OutQuad,
                    onFinish: EnableMovement
                );
            }
            else OnCarryingRotationCancelled?.Invoke();
        }
    }

    /// <summary>Enables character movement by allowing user input to be processed.</summary>
    public void EnableMovement() => IsUsingInput = true;

    /// <summary>Disables character movement by ignoring user input.</summary>
    public void DisableMovement() => IsUsingInput = false;


    /// <summary>Creates a new path point based on player input.</summary>
    /// <returns>The difference between the current position and the new path point position.</returns>
    Vector3 CreateNewPathPoint()
    {
        OnNewPathPoint?.Invoke();

        Vector3 addToPosition = new(input.Movement.x, 0, input.Movement.y);
        movementAxis = input.Axis;

        currentSnap.position = towardsSnap.position;
        towardsSnap.position += addToPosition;

        return addToPosition;
    }

    /// <summary>Rotates the player model.</summary>
    /// <param name="target">The target direction to rotate towards.</param>
    /// <param name="onFinish">An optional action to invoke when the rotation finishes.</param>
    public override void RotateModel(
        Vector2 target, float time, Ease? ease = null,
        Action onFinish = null)
    {
        // Get target rotation
        float targetDeg = Utils.Vectors.VecToDeg(target) - 90f;
        Vector3 euler = MeshModel.localEulerAngles;
        Quaternion targetRotation = Quaternion.Euler(euler.x, targetDeg, euler.z);

        // Prevent rotation if the rotation angle diff is too small or if it's already rotating to that angle
        if ((!IsRotating && Quaternion.Angle(MeshModel.localRotation, targetRotation) < Consts.MIN_ROTATION_DEG)
            || (IsRotating && Quaternion.Angle(rotatingTowards, targetRotation) < Consts.MIN_ROTATION_DEG))
        {
            OnRotationFinished?.Invoke();
            onFinish?.Invoke();
            return;
        }

        // Kill previous rotation tween and coroutine
        DOTween.Kill(TweenIds.PlayerRotate);
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }

        // Set rotation properties
        rotatingTowards = targetRotation;
        previousRotation = transform.rotation;
        Ease easing = ease ?? Ease.OutQuint;

        OnRotationStarted?.Invoke();
        IsRotating = true;
        IsActuallyRotating = true;

        // Rotate the model towards the target rotation
        MeshModel
            .DOLocalRotateQuaternion(targetRotation, time)
            .SetEase(easing)
            .SetId(TweenIds.PlayerRotate)
            .OnComplete(() => IsActuallyRotating = false);

        // Wait until the rotation is close enough to the target rotation
        rotationCoroutine = StartCoroutine(
            WaitForRotationToFinish(targetRotation, () =>
        {
            OnRotationCompleted?.Invoke();

            IsRotating = false;
            modelFacingDirection = target;

            OnRotationFinished?.Invoke();
            onFinish?.Invoke();
        }));
    }

    /// <summary>Waits until the rotation is close enough to the target rotation.</summary>
    IEnumerator WaitForRotationToFinish(Quaternion targetRotation, Action onFinish = null)
    {
        if (!IsRotating)
        {
            // If not rotating, invoke the finish action immediately
            onFinish?.Invoke();
            yield break;
        }

        // Wait until the rotation is close enough to the target rotation
        yield return new WaitUntil(() =>
            Quaternion.Angle(MeshModel.localRotation, targetRotation) < Config.RotationFinishMinAngle);

        onFinish?.Invoke();
    }

    /// <summary>Moves player back to the previous grid position.</summary>
    public void ResetPosition()
    {
        Vector3 targetPosition = currentSnap.position;
        MoveTowards(targetPosition);
        currentSnap.position = targetPosition;
    }

    /// <summary>Snaps player to the closest grid position ignoring y axis.</summary>
    public override void SnapPosition()
    {
        Vector3 targetPosition = Utils.Vectors.Round(transform.position);
        MoveTowards(targetPosition);
    }

    /// <summary>Moves player to the given position ignoring y axis.</summary>
    public override void MoveTowards(Vector3 newPosition, bool rotateModel = false)
    {
        Vector3 targetPosition = Utils.Vectors.Round(newPosition);
        targetPosition.y = towardsSnap.position.y;

        if (rotateModel)
        {
            Vector2 targetRotation = Utils.Vectors.CalcDirection(transform.position, targetPosition);
            RotateModel(targetRotation, Config.SoloRotationTime);
        }

        currentSnap.position = towardsSnap.position;
        towardsSnap.position = targetPosition;
        IsMoving = true;
    }

    /// <summary>
    /// Forces the player to move to the given position ignoring y axis.
    /// Automatically disables input and enables it again as soon as the player stops moving.
    /// </summary>
    public void ForceMoveTowards(Vector3 newPosition, bool rotateModel = false)
    {
        StartCoroutine(Sequence());
        IEnumerator Sequence()
        {
            DisableMovement();
            MoveTowards(newPosition, rotateModel);

            yield return new WaitUntil(() => !IsMoving);
            EnableMovement();
        }
    }

    /// <summary>Calculates and sets the current player velocity and axis velocity.</summary>
    void CalcVelocity()
    {
        Vector2 currentPosition = Utils.Vectors.IgnoreY(transform.position);
        Velocity = (currentPosition - previousPosition) / Time.deltaTime;
        previousPosition = currentPosition;

        AxisVelocity = movementAxis == Axis.Horizontal
            ? Velocity.x : Velocity.y;
    }

    protected override void OnCurrentCollidersRefresh()
    {
        base.OnCurrentCollidersRefresh();
        PressableChecker pressableChecker = new(this, CurrentColliders, PreviousColliders);
        pressableChecker.Execute();
    }

    private void OnDrawGizmos()
    {
        if (showSnapGizmos)
        {
            float RAD = 0.2f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(towardsSnap.position, RAD);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentSnap.position, RAD);
        }
    }

    enum TweenIds
    {
        PlayerRotate,
    }
}
