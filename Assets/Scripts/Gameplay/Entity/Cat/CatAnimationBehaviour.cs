using DG.Tweening;
using UnityEngine;

public class CatAnimationBehaviour : StateMachineBehaviour
{
    [SerializeField] protected CatFaces faceType = CatFaces.Normal;
    [SerializeField] protected bool freezing;
    [SerializeField] protected bool heating;

    protected readonly string FACE_INDEX_NAME = "_FaceIndex";
    protected readonly string FREEZING_NAME = "_FreezingAmount";
    protected readonly string HEATING_NAME = "_HeatingAmount";

    Material catMaterial;
    const float TEMPERATURE_TRANSITION_DURATION = 0.9f;

    public enum CatFaces
    {
        Normal,
        Happy,
        Sad,
        Surprised,
        Yawning,
        Sleepy,
        Sleeping
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get cat material
        if (GameManager.I == null)
        {
            Debug.LogWarning("GameManager not ready in CatAnimationStateBehaviour. Skipping shader setup.");
            return;
        }
        catMaterial = GameManager.I.StaticConfig.Materials.Cat;

        // Swap face
        catMaterial.SetFloat(FACE_INDEX_NAME, (int)faceType);

        // Set freezing and heating
        SetTemperature(catMaterial, FREEZING_NAME, freezing);
        SetTemperature(catMaterial, HEATING_NAME, heating);
    }

    void SetTemperature(Material catMaterial, string propertyName, bool enable)
    {
        int targetValue = enable ? 1 : 0;
        float prevValue = catMaterial.GetFloat(propertyName);
        if (prevValue == targetValue) return;

        catMaterial
            .DOFloat(targetValue, propertyName, TEMPERATURE_TRANSITION_DURATION)
            .SetEase(Ease.InOutSine);
    }
}
