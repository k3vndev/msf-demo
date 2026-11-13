using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;

public class GenerateLevelGroundTiles : EditorWindow
{
    static readonly string prefix = "GenerateLevelGroundTiles";

    [MenuItem("Tools/Level/Generate/Ground Tiles")]

    public static void Main()
    {
        LevelManager levelManager = LevelManager.TryGet(ErrorMessage("No Level Manager was found on the scene!"));
        if (levelManager == null) return;

        GameObject levelGrid = GameObject.FindWithTag("LevelGrid");
        if (levelGrid == null)
        {
            Debug.LogWarning(ErrorMessage("No Level Grid was found on the scene!"));
            return;
        }

        Tilemap tilemap = levelGrid.transform.GetComponentInChildren<Tilemap>();
        if (tilemap?.name != "Layer 0")
        {
            Debug.LogWarning(ErrorMessage("Invalid setup, no Tilemap named \"Layer 0\" was found as the first child of Level Grid!"));
            return;
        }

        RuleTile ruleTileBase = AssetDatabase.LoadAssetAtPath<RuleTile>("Assets/Settings/Tile Map/snow-ground_rule-tile.asset");

        // Clear existing tiles before generating new ones
        tilemap.ClearAllTiles();

        // Iterate through level tile positions and set tiles
        LevelManager.IterateLevelTiles(levelManager, pos =>
        {
            Vector3 rawPos = new(pos.x, 0, pos.y);

            // Skip tile if there's ice at this position
            if (Utils.IsColliding(rawPos, LayerMasks.Ice.Mask)) return;

            // Place tile at the calculated cell position
            Vector3Int cellPos = tilemap.WorldToCell(rawPos);
            tilemap.SetTile(cellPos, ruleTileBase);
        });

        Debug.Log($"{prefix}: Level ground tiles generated successfully.");

        // Generate Ice Chunk
        string iceChunkName = "Level/Ice Chunk";

        if (GameObject.Find(iceChunkName) is GameObject existingIceChunk)
            DestroyImmediate(existingIceChunk);

        GameObject iceChunkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Nature/Ice Chunk.prefab");
        GameObject iceChunk = (GameObject)PrefabUtility.InstantiatePrefab(iceChunkPrefab);
        iceChunk.name = iceChunkName;

        float scaleFactor = 0.08f;
        float fullZSize = LevelManager.PaddedBounds.PaddingBot + LevelManager.LevelDepth + LevelManager.PaddedBounds.PaddingTop;
        float zPosition = fullZSize / 2 - (LevelManager.LevelDepth / 2 + LevelManager.PaddedBounds.PaddingBot) - 0.5f;
        float zScale = (fullZSize - 1) * scaleFactor;

        float fullXSize = LevelManager.PaddedBounds.PaddingX * 2 + levelManager.LevelLength;
        float xPosition = fullXSize / 2 - LevelManager.PaddedBounds.PaddingX - 0.5f;
        float xScale = (fullXSize + 1f) * scaleFactor;

        iceChunk.transform.localPosition = new Vector3(xPosition, 0, zPosition);
        iceChunk.transform.localScale = new Vector3(xScale, 1, zScale);

        EditorSceneManager.MarkAllScenesDirty();
    }

    public static string ErrorMessage(string reason)
        => $"{prefix}: Couldn't generate level ground tiles. {reason}";
}