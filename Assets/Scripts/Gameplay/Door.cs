using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Linq;

public class Door : MonoBehaviour
{
    [SerializeField] float openedPosition = -0.97f;

    [Header("Animation")]
    [SerializeField] float moveTime;
    [SerializeField] float openTimePercent = 0.3f;
    float closedPosition;

    [Space]
    [SerializeField] Transform meshModel;

    public bool IsOpen { get; private set; } = false;

    bool isWaitingToClose = false;
    bool isOpening = false;
    Coroutine currentChangeValueSequence;
    string openCloseTweenId;

    void Awake()
    {
        openCloseTweenId = $"door-move-{GetInstanceID()}";
        closedPosition = meshModel.localPosition.y;
    }

    void MoveModel(float targetY)
    {
        DOTween.Kill(openCloseTweenId);
        meshModel
            .DOLocalMoveY(targetY, moveTime)
            .SetEase(Ease.OutCubic)
            .SetId(openCloseTweenId);
    }

    IEnumerator OnChangeValueSequence(bool wannaOpen)
    {
        if (wannaOpen)
        {
            // Open instantly and wait for animation to be halfway to set IsOpen as true
            MoveModel(openedPosition);
            isOpening = true;

            yield return new WaitForSeconds(moveTime * openTimePercent);

            IsOpen = true;
            isOpening = false;

            yield break;
        }

        if (DoesntHaveObstaclesOver())
        {
            // Close door instantly
            IsOpen = false;
            MoveModel(closedPosition);
        }
        else
        {
            // Wait until entities count reachs zero 
            isWaitingToClose = true;
            MoveModel(openedPosition);
            PlayShakeAnimation();

            yield return new WaitUntil(DoesntHaveObstaclesOver);

            isWaitingToClose = false;
            IsOpen = false;
            MoveModel(closedPosition);
        }
    }

    void OnChangeValue(bool wannaOpen)
    {
        if (isWaitingToClose && wannaOpen)
        {
            StopAnimation();
            isWaitingToClose = false;
            return;
        }

        if ((isOpening && !wannaOpen) || (wannaOpen != IsOpen))
        {
            StopAnimation();
            currentChangeValueSequence = StartCoroutine(OnChangeValueSequence(wannaOpen));
        }
    }

    void PlayShakeAnimation()
    {
        // Animation here
    }

    void StopAnimation()
    {
        if (currentChangeValueSequence != null)
            StopCoroutine(currentChangeValueSequence);

        DOTween.Kill(openCloseTweenId);
    }

    bool DoesntHaveObstaclesOver()
    {
        Physics.SyncTransforms();

        return Utils
            .GetCollidersAt(transform.position)
            .All(collider => // Ensure all colliders are not obstacles
            {
                // If the collider is this same Door, its not an obstacle
                if (collider.GetComponent<Door>() == this) return true;

                // If the collider has a MotionEntity component, it's considered an obstacle
                if (collider.GetComponent<MotionEntity>()) return false;

                // If the collider's layer is included in the obstacle mask, it's an obstacle
                LayerMask obstacles = LayerMasks.Obstacle.Mask | LayerMasks.Player.Mask;
                if (Utils.IsInLayerMask(collider.gameObject.layer, obstacles)) return false;

                // Otherwise, not an obstacle
                return true;
            });
    }

    void OnEnable() => MechanismMaterials.Stone.OnChange += OnChangeValue;
    void OnDisable() => MechanismMaterials.Stone.OnChange -= OnChangeValue;
}
