using UnityEngine;
using UnityEditor;

public class TileModelBoundsPostProcessor : AssetPostprocessor
{
    static readonly float FACTOR = 4.9f;

    void OnPostprocessModel(GameObject g)
    {
        if (!g.name.EndsWith("_tile")) return;

        foreach (var meshFilter in g.GetComponentsInChildren<MeshFilter>())
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null) continue;

            Vector3 originalSize = mesh.bounds.size;
            mesh.bounds = new(mesh.bounds.center, originalSize * FACTOR);

            Debug.Log($"Updated bounds for {g.name} from [{originalSize}] to {mesh.bounds.size}");
        }
    }
}
