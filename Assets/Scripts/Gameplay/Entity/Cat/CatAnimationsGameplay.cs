using UnityEngine;

public class CatAnimationsGameplay : CatAnimations
{
    [Space, SerializeField] SledController sledController;
    PlayerMovement playerMovement;
    PlayerAnimations playerAnimations;

    bool _isOnTemperatureAnimation = false;
    bool _isDancing = false;

    protected override void Start()
    {
        playerMovement = Utils.GetComp<PlayerMovement>("Player");
        playerAnimations = playerMovement.GetComponent<PlayerAnimations>();
    }

    protected override void Update()
    {
        // Handle dancing animation
        _isDancing = playerAnimations.IsMoving && sledController.IsBeingCarried && !playerMovement.IsGoingBackwards;
        animator.SetBool(ANIM_NAMES.IsDancing, _isDancing);

        // Handle idle process (and sleeping animation)
        if (!_isDancing && !sledController.IsBeingCarried
            && !sledController.IsSliding && sledController.IsGrounded && !_isOnTemperatureAnimation)
        {
            StartIdleProcess();
            return;
        }
        StopIdleProcess();
    }

    public void RefreshTemperatureAnimations()
    {
        // Check if the cat is on a snowy pad or a burning pad
        bool isOnSnowyPad = false;
        bool isOnBurningPad = false;

        foreach (Collider collider in sledController.CurrentColliders)
        {
            if (collider.gameObject.GetComponent<SnowyPad>() != null)
                isOnSnowyPad = true;

            else if (collider.gameObject.GetComponent<BurningPad>()?.PadIsActive == true)
                isOnBurningPad = true; // Only consider the burning pad if it's active
        }

        // Set animation bools for heating/freezing
        animator.SetBool(ANIM_NAMES.IsFreezing, isOnSnowyPad);
        animator.SetBool(ANIM_NAMES.IsHeating, isOnBurningPad);

        // Handle triggers
        if (isOnSnowyPad && !_isOnTemperatureAnimation)
        {
            animator.SetTrigger(ANIM_NAMES.Freeze);
            _isOnTemperatureAnimation = true;
        }
        else if (isOnBurningPad && !_isOnTemperatureAnimation)
        {
            animator.SetTrigger(ANIM_NAMES.Heat);
            _isOnTemperatureAnimation = true;
        }
        else if (!isOnSnowyPad && !isOnBurningPad)
        {
            _isOnTemperatureAnimation = false;
        }
    }

    void OnEnable()
    {
        sledController.CurrentCollidersChanged += RefreshTemperatureAnimations;
    }

    void OnDisable()
    {
        sledController.CurrentCollidersChanged -= RefreshTemperatureAnimations;
    }
}
