using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;

public class ResetPlayerAndSledPosition
{
    static readonly string prefix = "ResetPlayerAndSledPosition";

    [MenuItem("Tools/Level/Reset Player And Sled Position")]
    public static void Main()
    {
        GameObject player = GameObject.FindWithTag("Player");
        GameObject sled = GameObject.FindWithTag("Sled");

        if (!player || !sled)
        {
            throw new Exception($"{prefix}: Player or Sled missing!");
        }

        player.transform.position = new(1, 1, 0);
        sled.transform.position = new(3, 1, 0);

        Debug.Log($"{prefix}: Player and Sled set to their initial positions.");
        EditorSceneManager.MarkAllScenesDirty();
    }
}
