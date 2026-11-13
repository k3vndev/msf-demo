using Unity.Cinemachine;
using UnityEngine;

public class MainMenuCamerasManager : MonoBehaviour
{
    [SerializeField] CinemachineCamera playerCamera;
    [SerializeField] CinemachineCamera catCamera;

    void RefreshCameras()
    {
        if (MainMenuManager.I != null)
        {
            if (MainMenuManager.I.IsOnSettingsMenu)
            {
                catCamera.Priority = 10;
                playerCamera.Priority = 0;
            }
            else
            {
                catCamera.Priority = 0;
                playerCamera.Priority = 10;
            }
        }
    }

    void OnEnable() => MainMenuManager.OnSwap += RefreshCameras;
    void OnDisable() => MainMenuManager.OnSwap -= RefreshCameras;
}
