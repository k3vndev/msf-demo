using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager I { get; private set; }
    public const int LevelDepth = 11;

    [field: SerializeField, Range(30, 100)]
    public int LevelLength { get; private set; }

    [SerializeField] AudioClip levelMusic;
    [SerializeField, Range(0f, 3f)] float musicStartWait = 0.5f;

    public bool HasStarted { get; private set; } = false;
    public bool HasFinished { get; private set; } = false;
    public bool IsRunning { get => HasStarted && !HasFinished; }

    public const int LevelsCount = 4;
    public const int InitialLevel = 1;
    const float PauseMenuMusicVolume = 0.1f;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;

    }

    void Start()
    {
        GameManager.SetCursorVisibility(false);
        SaveCurrentLevel();

        if (levelMusic != null)
            StartCoroutine(StartMusicSequence());
    }

    IEnumerator StartMusicSequence()
    {
        MusicManager.I.PlayAmbience();

        yield return new WaitUntil(() =>
            TransitionManager.I?.IsTransitioning == false
            || TransitionManager.I?.IsAnyTransitionerRunning() == false
        );
        yield return new WaitForSeconds(musicStartWait);
        MusicManager.I.PlayMusic(levelMusic);
    }

    /// <summary>Checks wether a position is within current level bounds ignoring y axis.</summary>
    public bool IsInsideBounds(Vector3 p)
    {
        return p.x >= -0.5f && p.x <= LevelLength - 0.5f
            && p.z <= LevelDepth / 2f && p.z >= -LevelDepth / 2f;
    }

    void SaveCurrentLevel()
    {
        int currentLevel = Scenes.GetCurrentLevelFromScene();
        if (currentLevel < InitialLevel || currentLevel > LevelsCount) return;

        GameManager.I.Global.CurrentLevel = currentLevel;
        SaveSystem.I.RequestSave();
    }

    void OnLevelSequencerFinishStartingSequence()
    {
        HasStarted = true;
    }

    void OnLevelSequencerStartExitingSequence()
    {
        MusicManager.I.StopMusic(MusicManager.FadeOutTime.Slow);
        HasFinished = true;
    }

    void OnTogglePause(bool paused)
    {
        if (MusicManager.I.CurrentMusic != null)
        {
            var newVolume = paused ? PauseMenuMusicVolume : MusicManager.I.MusicDefaultVolume;
            MusicManager.I.CurrentMusic.AudioSource.volume = newVolume;
        }
    }

    void OnEnable()
    {
        // Level sequencer
        LevelSequencer.OnFinishEntranceSequence += OnLevelSequencerFinishStartingSequence;
        LevelSequencer.OnStartExitSequence += OnLevelSequencerStartExitingSequence;

        // Music handling
        PauseMenuManager.OnTogglePause += OnTogglePause;
        PauseMenuActions.OnRestartLevel += MusicManager.I.StopMusic;
        PauseMenuActions.OnQuitLevel += MusicManager.I.StopMusic;
    }

    void OnDisable()
    {
        // Level sequencer
        LevelSequencer.OnFinishEntranceSequence -= OnLevelSequencerFinishStartingSequence;
        LevelSequencer.OnStartExitSequence -= OnLevelSequencerStartExitingSequence;

        // Music handling
        PauseMenuManager.OnTogglePause -= OnTogglePause;
        PauseMenuActions.OnRestartLevel -= MusicManager.I.StopMusic;
        PauseMenuActions.OnQuitLevel -= MusicManager.I.StopMusic;
    }

    public static class LevelScene
    {
        /// <returns>The scene index of the current level.</returns>
        public static int GetCurrentIndex()
        {
            int currentLevelIndex = GameManager.I.Global.CurrentLevel;
            return Scenes.GetLevelIndex(currentLevelIndex);
        }


        /// <summary>Increases <c>GlobalState.CurrentLevel</c> by 1 if possible.</summary>
        /// <returns>The scene index of the next level.</returns>
        public static int GetNextIndex()
        {
            int nextLevel = GameManager.I.Global.CurrentLevel + 1;

            if (nextLevel <= LevelsCount)
                return Scenes.GetLevelIndex(nextLevel);

            return -1;
        }
    }

#if UNITY_EDITOR
    public static LevelManager TryGet(string warningMessage = "")
    {
        try
        {
            return Utils.GetComp<LevelManager>("LevelManager");
        }
        catch (Exception ex)
        {
            if (warningMessage != "")
                Debug.LogWarning($"Couldn't find LevelManager. {warningMessage}\nException: {ex.Message}");

            return null;
        }
    }

    /// <summary>
    /// Defines padded bounds for level editing tools. <br/>
    /// This is larger than the actual level bounds to allow level edges decoration.
    /// </summary>
    public static class PaddedBounds
    {
        public const int PaddingBot = 4;
        public const int PaddingTop = 13;
        public const int PaddingX = 10;

        /// <summary>Base displacement on Z axis to center the level at Z = 0.</summary>
        public const int BaseDisplacementZ = (LevelDepth - 1) / 2;


        /// <summary>Gets the minimum X coordinate of the padded bounds.</summary>
        public static int GetMinX() => -1 - PaddingX;
        /// <summary>Gets the maximum X coordinate of the padded bounds.</summary>
        public static int GetMaxX(int levelLength) => levelLength + PaddingX;

        /// <summary>Gets the minimum Z coordinate of the padded bounds.</summary>
        public static int GetMinZ() => -BaseDisplacementZ - PaddingBot;
        /// <summary>Gets the maximum Z coordinate of the padded bounds.</summary>
        public static int GetMaxZ() => BaseDisplacementZ + PaddingTop;

        /// <summary>Gets the full size on X axis of the padded bounds.</summary>
        public static int GetFullX(int levelLength) => GetMaxX(levelLength) - GetMinX();
        /// <summary>Gets the full size on Z axis of the padded bounds.</summary>
        public static int GetFullZ() => GetMaxZ() - GetMinZ();
    }


    /// <summary>Iterates through all level tile positions and performs the given action.</summary>
    public static void IterateLevelTiles(LevelManager instance, Action<Vector2Int> action)
    {
        int levelLength = instance.LevelLength;
        for (int i = PaddedBounds.GetMinX(); i <= PaddedBounds.GetMaxX(levelLength); i++) // Columns
        {
            for (int j = PaddedBounds.GetMinZ(); j <= PaddedBounds.GetMaxZ(); j++) // Rows (item per column)
            {
                Vector2Int pos = new(i, j);
                action(pos);
            }
        }
    }
#endif
}
