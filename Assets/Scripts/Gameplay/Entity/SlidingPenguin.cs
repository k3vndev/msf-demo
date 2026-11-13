using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SlidingPenguin : SlidableEntity
{
    enum SlideTowards { Automatic, Forward, Right, Backward, Left }

    [Header("SlidingPenguin")]
    [SerializeField] SlideTowards slideTowards = SlideTowards.Automatic;

    [SerializeField] float movementSpeed = 1.5f;
    public bool IsMoving { get; private set; } = false;
    Vector2 iceDirection;
    [SerializeField] Animator animator;

    [Space, SerializeField] float rotateTime = 2.1f;
    [SerializeField] Ease rotateEasing;
    [Tooltip("The time to stop the rotation animation before the actual rotation finishes. Used to make the rotation animation look better by not stopping it too early or too late. Should be less than rotateTime.")]
    [SerializeField] float rotateAnimationTimeMinus = 0.35f;
    public bool IsRotating { get; private set; } = false;

    [Space, SerializeField] float beforeSlideWaitTime = 0.8f;
    [SerializeField] float beforeStartAnimationWaitTime = 0.3f;
    [SerializeField] float afterSlideWaitTime = 1.1f;

    /*
        NOTES (TODO):
        - This script's name will be changed to "PenguinController" in the future, as it will also handle the penguin's movement when it's not sliding, such as walking around.
        - Sliding detection and logic will be improved in the future. While penguins are not intended to interact with the player under any circumstances, collisions between penguins will still be possible and has to be addressed.
        - Penguin's AI for walking around will be implemented mainly with another script that will be reused for other animals, such as mountain goats.
    */

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(ProcessSequence());
    }

    protected override void Update()
    {
        base.Update();
    }

    IEnumerator ProcessSequence()
    {
        yield return null;

        while (true)
        {
            // Get the direction of the nearby ice
            Vector2? _nearbyIceDirection = GetNearbyIceDirection();
            if (_nearbyIceDirection == null) yield break;
            Vector2 position = Utils.Vectors.IgnoreY(transform.position);
            iceDirection = _nearbyIceDirection.Value;
            Vector2 target = position + iceDirection;

            // Rotate towards the ice
            RotateModel(iceDirection, rotateTime);

            // Handle rotation animations only if it started rotating
            // Note: Walking animation is used for the rotation animation, as it makes the penguin move its legs while rotating
            if (IsRotating)
            {
                animator.SetBool(ANIM_NAMES.IsWalking, true);
                if (rotateAnimationTimeMinus < rotateTime)
                {
                    // Wait until the rotation animation should stop, but keep rotating until the rotation finishes
                    yield return new WaitForSeconds(rotateTime - rotateAnimationTimeMinus);
                    animator.SetBool(ANIM_NAMES.IsWalking, false);
                }
                // Wait until the rotation finishes and then stop the walking animation (if it hasn't been stopped already)
                yield return new WaitUntil(() => !IsRotating);
                animator.SetBool(ANIM_NAMES.IsWalking, false);
            }
            yield return new WaitForSeconds(beforeSlideWaitTime);

            // Start playing the sliding animation
            animator.SetBool(ANIM_NAMES.IsSliding, true);
            yield return new WaitForSeconds(beforeStartAnimationWaitTime);

            // Move to the ice and wait until it finishes moving
            MoveTowards(target);
            yield return new WaitUntil(() => !IsMoving);

            bool startedToSlide = RequestStartSliding(iceDirection);
            if (!startedToSlide)
            {
                animator.SetBool(ANIM_NAMES.IsSliding, false);
                yield break;
            }

            yield return new WaitUntil(() => !IsSliding);
            animator.SetBool(ANIM_NAMES.IsSliding, false);
            yield return new WaitForSeconds(afterSlideWaitTime);
        }
    }

    Vector2? GetNearbyIceDirection()
    {
        if (slideTowards != SlideTowards.Automatic)
        {
            int index = (int)slideTowards - 1;
            Vector2 manualDir = Utils.Vectors.FromIndexToDirectional(index);

            // Checks if its colliding with ice before returning the direction. 
            return CheckCollidingWithIce(manualDir) ? manualDir : null;
        }

        // Check if theres ice on each main-axis direction
        for (int i = 0; i < 4; i++)
        {
            Vector2 autoDir = Utils.Vectors.FromIndexToDirectional(i);
            if (CheckCollidingWithIce(autoDir)) return autoDir;
        }
        return null;
    }

    bool CheckCollidingWithIce(Vector2 dir)
    {
        Vector3 originalPos = transform.position;
        originalPos.y = 0;
        Vector3 checkerPosition = new(originalPos.x + dir.x, originalPos.y, originalPos.z + dir.y);

        return Utils.IsColliding(checkerPosition, LayerMasks.Ice.Mask);
    }

    public override void MoveTowards(Vector3 target, bool rotateModel = false)
    {
        IsMoving = true;
        float distance = Utils.Vectors.DistanceIgnoreY(target, transform.position);
        float moveTime = distance / movementSpeed;

        transform.DOKill();
        transform
            .DOMove(target, moveTime)
            .SetEase(Ease.Linear)
            .OnComplete(() => IsMoving = false);
    }

    public override void RotateModel(Vector2 target, float time, Ease? ease = null, Action onFinish = null)
    {
        float targetDeg = Utils.Vectors.VecToDeg(target);
        Vector3 euler = MeshModel.eulerAngles;
        Quaternion m_target = Quaternion.Euler(euler.x, targetDeg, euler.z);

        if (Quaternion.Angle(MeshModel.rotation, m_target) < Consts.MIN_ROTATION_DEG)
        {
            onFinish?.Invoke();
            return;
        }

        Ease easing = ease ?? rotateEasing;
        IsRotating = true;

        MeshModel.DOKill();
        MeshModel
            .DORotateQuaternion(m_target, time)
            .SetEase(easing)
            .OnComplete(() =>
            {
                onFinish?.Invoke();
                IsRotating = false;
            });
    }

    static class ANIM_NAMES
    {
        public const string IsSliding = "is-sliding";
        public const string IsWalking = "is-walking";
    }
}
