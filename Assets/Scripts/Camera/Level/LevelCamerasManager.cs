using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LevelCamerasManager : MonoBehaviour
{
    public static LevelCamerasManager I;
    public float BoundsGapX = 8f;
    [SerializeField] CinemachineBrain cinemachineBrain;

    [Header("Cameras")]
    [SerializeField] LevelFollowCamera followCamera;
    [SerializeField] LevelDollyCamera dollyCamera;

    [Header("Blending")]
    [SerializeField] float defaultBlendTime = 1.6f;
    [SerializeField] CinemachineBlendDefinition.Styles blendStyle;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(this);
            return;
        }
        I = this;

        EnableBlending();
    }

    public void Dolly()
    {
        dollyCamera.CMCamera.Priority = 1;
        followCamera.CMCamera.Priority = -1;
    }

    public void Follow()
    {
        followCamera.CMCamera.Priority = 1;
        dollyCamera.CMCamera.Priority = -1;
    }

    public void DollyAnimation()
        => StartCoroutine(DollyAnimationSequence());

    IEnumerator DollyAnimationSequence()
    {
        DisableBlending();
        Dolly();

        yield return new WaitUntil(() =>
            cinemachineBrain.ActiveVirtualCamera != null &&
            cinemachineBrain.ActiveVirtualCamera.Equals(dollyCamera.CMCamera));

        EnableBlending();
        Follow();
    }

    public void EnableBlending()
    {
        cinemachineBrain.DefaultBlend.Style = blendStyle;
        cinemachineBrain.DefaultBlend.Time = defaultBlendTime;
    }

    public void DisableBlending()
    {
        cinemachineBrain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Cut;
        cinemachineBrain.DefaultBlend.Time = 0;
    }
}
