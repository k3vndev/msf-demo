using Unity.Cinemachine;
using UnityEngine;

public abstract class LevelCamera : MonoBehaviour
{
    public CinemachineCamera CMCamera { get; private set; }

    protected virtual void Awake()
    {
        CMCamera = GetComponent<CinemachineCamera>();
    }
}
