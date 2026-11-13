using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PositionateLevelDoors : EditorWindow
{
    static readonly string prefix = "PositionateLevelDoors";

    [MenuItem("Tools/Level/Positionate Level Doors")]
    public static void Main()
    {
        string errorMsgStart = $"{prefix}: Couldn't positionate level doors";

        // Get level manager
        LevelManager levelManager = LevelManager.TryGet($"{errorMsgStart}, no LevelManager was found on the scene!");
        if (levelManager == null) return;

        // Get level sequencer
        if (GameObject.Find("Level Sequencer")?.GetComponent<LevelSequencer>()
            is not LevelSequencer levelSequencer)
        {
            Debug.LogWarning($"{errorMsgStart}, no LevelSequencer was found on the scene!");
            return;
        }

        // Execute positioning logic
        levelSequencer.transform.position = Vector3.zero;
        levelSequencer.EntranceDoor.transform.position = new Vector3(-2, 1, 0);
        levelSequencer.ExitDoor.transform.position = new Vector3(levelManager.LevelLength + 1, 1, 0);

        Debug.Log($"{prefix}: Level doors positionated!");
        EditorSceneManager.MarkAllScenesDirty();
    }
}