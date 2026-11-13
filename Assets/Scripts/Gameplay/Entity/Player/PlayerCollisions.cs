using System;
using System.Linq;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    [SerializeField] LayerMask obstaclesLayers;

    [field: Header("Collision Checkers")]
    [field: SerializeField] public Transform MoveCollisionChecker { get; private set; }
    [field: SerializeField] public Transform MoveGroundCollisionChecker { get; private set; }
    [field: SerializeField] public Transform RotateCollisionChecker { get; private set; }

    SledController sledController;
    PlayerController controller;
    PlayerInput input;
    PlayerMovement movement;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();

        // Initialize collision checker transforms
        Utils.PrefixAndUnParent("Player", MoveCollisionChecker, MoveGroundCollisionChecker, RotateCollisionChecker);
    }

    void Start()
    {
        sledController = Utils.GetComp<SledController>("Sled");
    }

    void Update()
    {
        SledRaycast sledRaycast = new(transform.position, sledController.transform.position, 1);
        controller.SetSledIsOnRange(sledRaycast.IsHitting());
    }

    public bool WouldCollideAtMove()
        => WouldCollideAt(MoveCollisionChecker.position);

    public bool WouldCollideAtRotate()
        => WouldCollideAt(RotateCollisionChecker.position);

    bool WouldCollideAt(Vector3 position)
    {
        Physics.SyncTransforms();
        foreach (Collider collider in Utils.GetCollidersAt(position))
        {
            // If collider is in obstacles layer
            if (Utils.IsInLayerMask(collider.gameObject.layer, obstaclesLayers)) return true;

            // If collider is a Frame and player is big or is carrying the sled
            if (collider.CompareTag("Frame")
                && (controller.CurrentSize == PlayerController.Size.Big || sledController.IsBeingCarried))
            {
                return true;
            }

            // If collider is a Pad and is not available
            Pad pad = collider.GetComponent<Pad>();
            if (pad != null && !pad.CanBeSteppedOver) return true;

            // If collider is a Door that is closed
            Door door = collider.GetComponent<Door>();
            if (door != null && !door.IsOpen) return true;

            // If collider is a LevelDoor that is closed
            LevelDoor levelDoor = collider.GetComponent<LevelDoor>();
            if (levelDoor != null && !levelDoor.IsOpen) return true;
        }
        return false;
    }

    public bool WouldStepOnIce()
    {
        RefreshMoveGroundChecker();
        MoveGroundCollisionChecker.position -= Vector3.up;
        return Utils.IsColliding(MoveGroundCollisionChecker.position, LayerMasks.Ice.Mask);
    }

    /// <summary>Refreshes and returns the move collision checker.</summary>
    public Transform RefreshMoveChecker()
        => RefreshChecker(MoveCollisionChecker);

    /// <summary>Refreshes and returns the move ground collision checker.</summary>
    public Transform RefreshMoveGroundChecker()
        => RefreshChecker(MoveGroundCollisionChecker);

    /// <summary>Refreshes and returns the rotate collision checker.</summary>
    public Transform RefreshRotateChecker()
        => RefreshChecker(RotateCollisionChecker);

    Transform RefreshChecker(Transform tr)
    {
        tr.gameObject.SetActive(true);
        Vector3 add = new(input.Movement.x, 0, input.Movement.y);
        tr.position = Utils.Vectors.Round(transform.position) + add;
        Physics.SyncTransforms();
        return tr;
    }

    void DisableCollisionCheckers()
    {
        Transform[] transforms = { MoveCollisionChecker, MoveGroundCollisionChecker, RotateCollisionChecker };

        foreach (Transform t in transforms)
            t.gameObject.SetActive(false);
    }

    private void HandleDisableRotationCollisionChecker()
    {
        if (!movement.IsMoving)
        {
            DisableCollisionCheckers();
            return;
        }
        RotateCollisionChecker.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        PlayerMovement.OnPathCompleted += DisableCollisionCheckers;
        PlayerMovement.OnRotationFinished += HandleDisableRotationCollisionChecker;
        PlayerMovement.OnCarryingRotationCancelled += DisableCollisionCheckers;
        PlayerMovement.OnNewPathPointCancelled += DisableCollisionCheckers;
    }

    void OnDisable()
    {
        PlayerMovement.OnPathCompleted -= DisableCollisionCheckers;
        PlayerMovement.OnRotationFinished -= HandleDisableRotationCollisionChecker;
        PlayerMovement.OnCarryingRotationCancelled -= DisableCollisionCheckers;
        PlayerMovement.OnNewPathPointCancelled -= DisableCollisionCheckers;
    }

    class SledRaycast
    {
        public readonly Vector3 Start, Direction;

        public SledRaycast(Vector3 position, Vector3 sledPosition, float length)
        {
            Vector2 dir = Utils.Vectors.CalcDirection(position, sledPosition);
            Vector3 rawDirection = new(dir.x, 0, dir.y);

            Start = position + rawDirection * (1 - length);
            Direction = rawDirection;
        }

        public bool IsHitting() => Physics
            .RaycastAll(Start, Direction, 1)
            .Any(r => r.collider.GetComponent<SledController>() != null);
    }
}
