using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    /// <summary>Stores the global state of the game.</summary>
    public GlobalState Global { get; private set; }

    /// <summary>Stores the static variables of the game.</summary>
    [field: SerializeField] public StaticConfig StaticConfig { get; private set; }

    void Awake()
    {
        if (I != null && I != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);

        InputManager.UseGameplay();
        Global = SaveSystem.SanitizeLoad(SaveSystem.Load());
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == Scenes.UI.LoadingScreen.Index)
        {
            LoadingScreen.I.StartLoading();
            LoadInitialScene();
        }
    }

    void LoadInitialScene()
    {
        float minTransitionDuration = 0.1f;

        if (Global.CurrentLevel == LevelManager.InitialLevel)
        {
            // If the current level from the save file is the initial level, load it directly.
            int levelIndex = Scenes.GetLevelIndex(LevelManager.InitialLevel);

            TransitionManager.I.Towards(levelIndex)
                .SetTransition(Transition.WhiteFade, Transition.None)
                .SetDuration(minTransitionDuration, 0f)
                .OnEarlyNewSceneLoaded(() =>
                {
                    LevelPresentation levelPresentation = new();
                    StartCoroutine(levelPresentation.Run());
                })
                .SkipLoadingScreen();
        }
        else
        {
            // If the current level is not the initial level, load the main menu.
            TransitionManager.I.Towards(Scenes.UI.MainMenu.Index)
                .SetTransition(Transition.WhiteFade, Transition.Circle)
                .SetDuration(minTransitionDuration, 0.8f)
                .SkipLoadingScreen();
        }
    }

    public static GameManager GetInstance()
    {
        return GameObject.FindWithTag("GameManager")?.GetComponent<GameManager>();
    }

    /// <summary>Enables or disables cursor visibility while keeping it always visible in Unity's editor.</summary>
    public static void SetCursorVisibility(bool value)
    {
#if !UNITY_EDITOR
        Cursor.visible = value;
#endif
    }
}
