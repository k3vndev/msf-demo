using DG.Tweening;
using UnityEngine;

public class PressurePlate : MonoBehaviour, IPlayerPressable, ISledPressable
{
    [SerializeField] Transform meshModel;
    [SerializeField] float pressedPosition;
    [SerializeField] float animationTime;
    float initialPosition;
    int pressingEntitiesCount;
    bool IsPressed { get => pressingEntitiesCount > 0; }

    void Awake()
    {
        initialPosition = meshModel.localPosition.y;
    }

    void RefreshModelPosition()
    {
        float target = IsPressed ? pressedPosition : initialPosition;

        meshModel.DOKill();
        meshModel
            .DOLocalMoveY(target, animationTime)
            .SetEase(Ease.InOutQuad);
    }

    void HandlePress()
    {
        pressingEntitiesCount++;

        MechanismMaterials.Stone.Activate();
        RefreshModelPosition();
    }

    void HandleRelease()
    {
        if (pressingEntitiesCount > 0)
            pressingEntitiesCount--;

        MechanismMaterials.Stone.Deactivate();
        RefreshModelPosition();
    }

    public void PlayerPress() => HandlePress();
    public void PlayerRelease() => HandleRelease();
    public void SledPress() => HandlePress();
    public void SledRelease() => HandleRelease();
}
