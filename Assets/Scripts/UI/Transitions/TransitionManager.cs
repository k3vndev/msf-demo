using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages scene transitions in the game. It provides methods to transition towards a specific scene, and handles the transitioners that are used to create visual effects during transitions.
/// </summary>
public class TransitionManager : MonoBehaviour
{
    public static TransitionManager I { get; private set; }
    public bool IsTransitioning { get; private set; } = false;

    [Header("Config")]
    [SerializeField] Camera overlayCamera;

    [field: Header("Transitioners")]
    [field: SerializeField] public WhiteFadeTransitioner WhiteFadeTransitioner { get; private set; }
    [field: SerializeField] public SnowFlakesTransitioner SnowFlakesTransitioner { get; private set; }
    [field: SerializeField] public CircleTransitioner CircleTransitioner { get; private set; }

    /// <summary>Lists all transitioners in the game.</summary>
    public Transitioner[] Transitioners => new Transitioner[] { WhiteFadeTransitioner, SnowFlakesTransitioner, CircleTransitioner };

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    ///<summary>Transitionates towards the desired scene.</summary>
    public TransitionRequest Towards(SceneTarget sceneTarget)
        => Towards(GetSceneIndexFromTarget(sceneTarget));

    ///<summary>Transitionates towards the desired scene.</summary>
    ///<remarks>Transition starts one frame after being called.</remarks>
    public TransitionRequest Towards(int sceneIndex)
    {
        TransitionRequest request = new(sceneIndex);
        StartCoroutine(RunTransitionRequest(request));
        return request;
    }

    IEnumerator RunTransitionRequest(TransitionRequest request)
    {
        IsTransitioning = true;
        yield return request.Run();
        IsTransitioning = false;
    }

    int GetSceneIndexFromTarget(SceneTarget target)
    {
        return target switch
        {
            SceneTarget.CurrentLevel => LevelManager.LevelScene.GetCurrentIndex(),
            SceneTarget.NextLevel => LevelManager.LevelScene.GetNextIndex(),
            _ => -1,
        };
    }

    public Transitioner RetrieveTransitioner(Transition transition)
    {
        if (transition == Transition.None) return null;

        int index = (int)transition - 1;
        return Transitioners[index];
    }

    /// <summary>Checks if any transitioner is currently running a transition.</summary>
    public bool IsAnyTransitionerRunning()
        => Transitioners.Any(t => t.IsTransitioning);

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            // Add transition overlay camera to the main camera stack
            MainCamera mainCamera = Utils.GetComp<MainCamera>("MainCamera");
            bool added = mainCamera.AddOverlayCamera(overlayCamera, MainCamera.OverlayCameraPriority.Transitions);

            if (!added) Debug.LogWarning($"TransitionManager: Tried to add transition overlay camera to the main camera stack, but it was already in the stack.");
        }
        catch
        {
            Debug.LogWarning($"TransitionManager: Couldn't apply overlay, scene \"{scene.name}\" does not have a main camera set.");
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
}
