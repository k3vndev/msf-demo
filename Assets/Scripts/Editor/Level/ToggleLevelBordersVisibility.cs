using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ToggleLevelBordersVisibility
{
    private static bool _isEnabled = false;

    static ToggleLevelBordersVisibility()
    {
        _isEnabled = EditorPrefs.GetBool("ShowLevelBorders", false);

        SceneView.duringSceneGui -= OnSceneGUI;
        if (_isEnabled) SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("Tools/Level/Toggle Borders Visibility %&b")]
    private static void Main()
    {
        _isEnabled = !_isEnabled;

        SceneView.duringSceneGui -= OnSceneGUI;
        if (_isEnabled) SceneView.duringSceneGui += OnSceneGUI;

        EditorPrefs.SetBool("ShowLevelBorders", _isEnabled);
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/Level/Toggle Borders Visibility %&b", true)]
    private static bool Validate()
    {
        Menu.SetChecked("Tools/Level/Toggle Borders Visibility", _isEnabled);
        return true;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Handles.color = Color.red;

        LevelManager levelManager = LevelManager.TryGet();
        if (levelManager == null) return;

        Vector3 size = new(levelManager.LevelLength, 1, LevelManager.LevelDepth);
        Vector3 position = new(((float)levelManager.LevelLength / 2) - 0.5f, 0.5f, 0);

        Handles.DrawWireCube(position, size);
    }
}
