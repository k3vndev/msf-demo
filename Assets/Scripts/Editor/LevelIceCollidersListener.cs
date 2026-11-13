using System;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;


[InitializeOnLoad]
public static class LevelIceCollidersListener
{
    static DateTime lastPaintTime;
    const float DebounceTimeMs = 50;

    static LevelIceCollidersListener()
    {
        Debug.Log("LevelIceCollidersListener was initialized! Any tilemap changes will trigger ice collider regeneration.");

        LevelManager levelManager = LevelManager.TryGet();
        if (levelManager == null) return;

        CustomBrush.OnTilesUpdated += brushTarget =>
        {
            // Extract the layer index from the brush target if possible
            int layerIndex = (int)(brushTarget?.transform.position.y ?? -1);
            if (layerIndex > 0) return; // Skip non-ground layers

            StopWaiting();

            lastPaintTime = DateTime.Now;
            EditorApplication.update += CheckDebounce;
        };
    }

    static void CheckDebounce()
    {
        if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= DebounceTimeMs)
        {
            StopWaiting();
            GenerateLevelIceColliders.Main();
        }
    }

    static void StopWaiting()
    {
        EditorApplication.update -= CheckDebounce;
    }
}