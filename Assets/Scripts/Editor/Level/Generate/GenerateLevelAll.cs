using UnityEditor;

public class GenerateLevelAll : EditorWindow
{
    [MenuItem("Tools/Level/Generate/All %&a")]
    public static void Main()
    {
        GenerateLevelGroundCollider.Main();
        GenerateLevelGroundTiles.Main();
        PositionateLevelDoors.Main();
        GenerateLevelIceColliders.Main();
    }
}
