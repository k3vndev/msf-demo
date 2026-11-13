using UnityEngine;
using UnityEditor;

public class SelectedPrefabsUtilities : EditorWindow
{
    Material mat;
    string prefix = "";

    float collidersSize = 1;

    [MenuItem("Tools/Open Selected Prefabs Utilities")]
    static void OpenWindow()
    {
        GetWindow<SelectedPrefabsUtilities>("Selected Prefabs Utilities");
    }

    private void OnGUI()
    {
        GUILayout.Label("Set Material", EditorStyles.boldLabel);

        mat = (Material)EditorGUILayout.ObjectField("Material", mat, typeof(Material), false);
        if (GUILayout.Button("Apply Material To Selected") && mat != null)
        {
            ApplyMaterialToSelectedObjects();
        }

        GUILayout.Space(14);
        GUILayout.Label("Prefix Selected", EditorStyles.boldLabel);

        prefix = EditorGUILayout.TextField("Prefix", prefix);
        if (GUILayout.Button("Prefix Selected"))
        {
            PrefixSelected();
        }

        GUILayout.Space(14);
        GUILayout.Label("Generate Colliders", EditorStyles.boldLabel);

        collidersSize = EditorGUILayout.FloatField("Size", collidersSize);
        if (GUILayout.Button("Generate Colliders"))
        {
            GenerateColliders();
        }
    }

    void ApplyMaterialToSelectedObjects()
    {
        // Get all selected assets in the Project window
        foreach (Object asset in Selection.objects)
        {
            // Get the asset path
            string path = AssetDatabase.GetAssetPath(asset);

            // Attempt to load the asset as a prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue; // Not a valid prefab

            // Load the prefab contents into a temporary scene
            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(path);
            if (contentsRoot == null) continue;

            // Find all renderers in the prefab and set the material
            foreach (MeshRenderer r in contentsRoot.GetComponentsInChildren<MeshRenderer>(true))
                r.material = mat;

            // Save changes back to the prefab asset
            PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
            PrefabUtility.UnloadPrefabContents(contentsRoot);

            Debug.Log($"Updated materials for prefab: {prefab.name}");
        }
    }

    void PrefixSelected()
    {
        foreach (Object asset in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            string newName = asset.name;

            // Remove previous prefix
            int underlineCount = 0;
            for (int i = newName.Length - 1; i >= 0; i--)
            {
                if (newName[i] == '_') underlineCount++;

                if (underlineCount >= 2)
                {
                    newName = newName[(i + 1)..];
                    break;
                }
            }

            // Add new prefix
            newName = $"{prefix}_{newName}";
            string errorMsg = AssetDatabase.RenameAsset(path, newName);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                Debug.LogError("Rename failed: " + errorMsg);
            }
            else
            {
                Debug.Log("Asset renamed to " + newName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    void GenerateColliders()
    {
        // Get all selected assets in the Project window
        foreach (Object asset in Selection.objects)
        {
            // Get the asset path
            string path = AssetDatabase.GetAssetPath(asset);

            // Attempt to load the asset as a prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue; // Not a valid prefab

            // Load the prefab contents into a temporary scene
            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(path);
            if (contentsRoot == null) continue;

            foreach (BoxCollider collider in contentsRoot.GetComponents<BoxCollider>())
                DestroyImmediate(collider);

            BoxCollider newCollider = contentsRoot.AddComponent<BoxCollider>();
            newCollider.isTrigger = true;
            newCollider.size = Vector3.one * collidersSize;
            contentsRoot.layer = LayerMasks.Wall.Index;

            // Save changes back to the prefab asset
            PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
            PrefabUtility.UnloadPrefabContents(contentsRoot);

            Debug.Log($"Set colliders for prefab: {prefab.name}");
        }
    }
}
