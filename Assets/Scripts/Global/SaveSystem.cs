using System;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem I { get; private set; }

    static string SavePath => Application.persistentDataPath + "/save.json";
    static string BackupPath => Application.persistentDataPath + "/save_backup.json";

    public static string GAME_VERSION;

    [SerializeField, Range(0, 2)] float cooldown = 0.55f;
    float nextSaveTime = 0f;
    bool isRequestingSave = false;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;

        GAME_VERSION = Application.version;
    }

    void Update()
    {
        if (Time.unscaledTime >= nextSaveTime && isRequestingSave)
        {
            isRequestingSave = false;
            InstantSave();
        }
    }

    /// <summary>
    /// Requests a save operation. Once the cooldown period has passed since 
    /// the las request, the game will be saved.
    /// </summary>
    public void RequestSave()
    {
        nextSaveTime = Time.unscaledTime + I.cooldown;
        isRequestingSave = true;
    }

    /// <summary>Saves the current game state immediately, regardless of cooldown.</summary>
    public void InstantSave()
    {
        GlobalState state = GameManager.I.Global;
        string json = JsonUtility.ToJson(state, true);

        // Save the current state to the save file and create a backup.
        File.WriteAllText(SavePath, json);
        File.Copy(SavePath, BackupPath, true);
    }

    /// <summary>
    /// Loads the game state from the save file, or from a backup if the save file is not found. <br/>
    /// If neither is available, returns a new GlobalState instance.
    /// </summary>
    public static GlobalState Load()
    {
        // Check for the save and backup files and load the first valid one found.
        string[] paths = { SavePath, BackupPath };
        foreach (string path in paths)
        {
            GlobalState state = LoadFromFile(path);
            // state.CurrentLevel = 1; // Nota: Quité el guardado de niveles para mejor experiencia en el "Arcade"
            if (state != null) return state;
        }

        // If no valid save or backup is found, return a new GlobalState
        return new GlobalState();
    }

    /// <summary>Loads the game state from a specified file path.</summary>
    static GlobalState LoadFromFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<GlobalState>(json);
            }
        }
        catch { }
        return null;
    }

    /// <summary>Sanitizes a loaded <c>GlobalState</c> object to ensure its usable by the game.</summary>
    /// <returns>The sanitized <c>GlobalState</c></returns>
    public static GlobalState SanitizeLoad(GlobalState load)
    {
        GlobalState sanit = load;

        // Sanitize current level
        sanit.CurrentLevel = Mathf.Clamp(sanit.CurrentLevel,
            LevelManager.InitialLevel, LevelManager.LevelsCount);

        // Sanitize last used tooltip
        if (!Enum.IsDefined(typeof(ControlsTooltip.TooltipName), load.LastUsedTooltip))
            sanit.LastUsedTooltip = ControlsTooltip.TooltipName.None;

        // Sanitize settings
        sanit.Settings.MusicVolume = Mathf.Clamp(sanit.Settings.MusicVolume, 0, 100);
        sanit.Settings.SFXVolume = Mathf.Clamp(sanit.Settings.SFXVolume, 0, 100);
        sanit.Settings.Quality = Mathf.Clamp(sanit.Settings.Quality, 0, 2);

        if (sanit.Settings.Language != 0 && sanit.Settings.Language != 1)
        {
            // Auto detect system language
            int newValue = Application.systemLanguage == SystemLanguage.Spanish ? 1 : 0;
            sanit.Settings.Language = newValue;
        }

        // Note: Windowed width and height don't need to be sanitized since ScreenResizer does it on get.

        return sanit;
    }
}
