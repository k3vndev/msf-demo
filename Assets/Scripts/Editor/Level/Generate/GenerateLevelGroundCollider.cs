using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GenerateLevelGroundCollider : EditorWindow
{
    static readonly string prefix = "GenerateLevelGroundCollider";
    static readonly string objName = "Level/GroundCollider";

    [MenuItem("Tools/Level/Generate/Ground Collider")]
    public static void Main()
    {
        LevelManager levelManager = LevelManager.TryGet(
            $"{prefix}: Couldn't generate level ground, No LevelManager was found on the scene!");
        if (levelManager == null) return;

        GameObject containerObj = Utils.Dev.FindOrCreateGameObject(
            objName, g => g.layer = LayerMasks.Snow.Index);

        BoxCollider[] boxColliders = containerObj.GetComponentsInChildren<BoxCollider>();
        foreach (BoxCollider box in boxColliders) DestroyImmediate(box);

        float posX = ((float)levelManager.LevelLength / 2) - 0.5f;

        BoxCollider boxCollider = containerObj.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(posX, 0, 0);
        boxCollider.size = new Vector3(levelManager.LevelLength + LevelManager.PaddedBounds.PaddingX * 2, 1, LevelManager.LevelDepth);

        Debug.Log($"{prefix}: Level ground collider generated!");
        EditorSceneManager.MarkAllScenesDirty();
    }
}
