using UnityEngine;
using UnityEditor;

public class TilemapShortcuts
{
    [MenuItem("Tools/Level/Tilemap Shortcuts/Select Tilemap 1 #1")]
    static void SelectTilemap1() => SelectTilemap(1);

    [MenuItem("Tools/Level/Tilemap Shortcuts/Select Tilemap 2 #2")]
    static void SelectTilemap2() => SelectTilemap(2);

    [MenuItem("Tools/Level/Tilemap Shortcuts/Select Tilemap 3 #3")]
    static void SelectTilemap3() => SelectTilemap(3);

    [MenuItem("Tools/Level/Tilemap Shortcuts/Select Tilemap 4 #4")]
    static void SelectTilemap4() => SelectTilemap(4);

    static void SelectTilemap(int tilemapLayerIndex)
    {
        // Find the "LevelGrid" GameObject in the scene
        GameObject levelGrid = GameObject.FindWithTag("LevelGrid");
        if (levelGrid == null)
        {
            WarnMessage("No GameObject with tag 'LevelGrid' found.");
            return;
        }

        // Try to select an existing tilemap layer child GameObject
        foreach (Transform child in levelGrid.transform)
        {
            string expectedName = $"Layer {tilemapLayerIndex}";

            if (child.name.Equals(expectedName))
            {
                SelectObject(child.gameObject);
                return;
            }
        }

        // If no existing tilemap layer was found, create a new one and select it
        WarnMessage($"No child GameObject named 'Layer {tilemapLayerIndex}' was found under 'LevelGrid'. Creating it now...");

        string name = $"Layer {tilemapLayerIndex}";
        Vector3 position = new(0, tilemapLayerIndex - 1, 0);

        GameObject newTilemapLayer = new(name);
        newTilemapLayer.transform.SetParent(levelGrid.transform);
        newTilemapLayer.transform.SetLocalPositionAndRotation(position, Quaternion.identity);
        newTilemapLayer.AddComponent<UnityEngine.Tilemaps.Tilemap>();
        newTilemapLayer.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();

        SelectObject(newTilemapLayer);
    }

    static void SelectObject(GameObject obj)
    {
        Selection.activeGameObject = obj;
        EditorGUIUtility.PingObject(obj);
    }

    static void WarnMessage(string message)
    {
        Debug.LogWarning($"TilemapShortcuts: {message}");
    }
}