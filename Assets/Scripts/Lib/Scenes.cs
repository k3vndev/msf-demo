
using UnityEngine.SceneManagement;

public static class Scenes
{
    readonly static int levelIndexOffset = 1;

    public class SceneRef
    {
        public readonly int Index;
        public readonly string Name;

        public SceneRef(int index, string name)
        {
            Index = index;
            Name = name;
        }
    }

    public static class UI
    {
        public static readonly SceneRef LoadingScreen = new(0, "UI - LOADING");
        public static readonly SceneRef MainMenu = new(1, "UI - MAIN MENU");
    }

    public static int GetLevelIndex(int level) => level + levelIndexOffset;

    // public static int GetCinematicIndex(int cinematic) => -1;

    public static int GetCurrentLevelFromScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        try
        {
            if (sceneName.StartsWith("LEVEL"))
            {
                int currentLevel = int.Parse(sceneName[^2..]);
                return currentLevel;
            }
            return -1;
        }
        catch { return -1; }
    }
}
