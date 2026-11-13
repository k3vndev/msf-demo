using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class SimpleOverlayCameraSetter : MonoBehaviour
{
    [SerializeField] MainCamera.OverlayCameraPriority priority;

    void Start()
    {
        try
        {
            MainCamera mainCamera = Utils.GetComp<MainCamera>("MainCamera");
            mainCamera.AddOverlayCamera(GetComponent<Camera>(), priority);
        }
        catch
        {
            Debug.LogWarning($"SimpleOverlayCameraSetter: Tried to add overlay camera to the main camera stack,"
                + $" but it was not found at {SceneManager.GetActiveScene().name}.");
        }
    }
}