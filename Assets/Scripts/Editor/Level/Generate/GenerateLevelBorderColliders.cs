using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public class GenerateLevelBorderColliders : EditorWindow
{
    static readonly string prefix = "GenerateLevelBorderColliders";
    static readonly string objName = "Level/BorderColliders";

    [MenuItem("Tools/Level/Generate/Border Colliders")]
    public static void Main()
    {
        LevelManager levelManager = LevelManager.TryGet(
            $"{prefix}: Couldn't generate level borders, No LevelManager was found on the scene!");
        if (levelManager == null) return;

        GameObject containerObj = Utils.Dev.FindOrCreateGameObject(objName);

        BoxCollider[] boxColliders = containerObj.GetComponentsInChildren<BoxCollider>();
        foreach (BoxCollider box in boxColliders) DestroyImmediate(box);

        containerObj.layer = LayerMasks.Wall.Index;

        float length = levelManager.LevelLength, depth = LevelManager.LevelDepth;
        float posX = length / 2, posY = 1, z = depth / 2;
        float verticalSize = (depth - 1) / 2, verticalPos = (depth + 1) / 4;

        BorderCollider[] borderColliders = {
            // Horizontal
            new(
                position: new Vector3(posX - 0.5f, posY, z + 0.5f),
                size: new Vector3(length, 1, 1)),
            new(
                position: new Vector3(posX - 0.5f, posY, -z - 0.5f),
                size: new Vector3(length, 1, 1)),

            // Vertical far
            new(
                position: new Vector3(-1f, posY, verticalPos),
                size: new Vector3(1, 1, verticalSize)),
            new(
                position: new Vector3(length, posY, verticalPos),
                size: new Vector3(1, 1, verticalSize)),

            // Vertical near
            new(
                position: new Vector3(-1f, posY, -verticalPos),
                size: new Vector3(1, 1, verticalSize)),
            new(
                position: new Vector3(length, posY, -verticalPos),
                size: new Vector3(1, 1, verticalSize))
        };

        foreach (BorderCollider border in borderColliders)
        {
            BoxCollider boxCollider = containerObj.AddComponent<BoxCollider>();
            boxCollider.size = border.Size;
            boxCollider.center = border.Position;
            boxCollider.isTrigger = true;
        }

        Debug.Log($"{prefix}: Level borders colliders generated!");
        EditorSceneManager.MarkAllScenesDirty();
    }

    class BorderCollider
    {
        public readonly Vector3 Position, Size;

        public BorderCollider(Vector3 position, Vector3 size)
        {
            Position = position;
            Size = size;
        }
    }
}
