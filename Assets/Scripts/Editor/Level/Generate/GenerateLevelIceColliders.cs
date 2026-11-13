using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/*
    Generates BoxColliders for all ice areas in the level by grouping adjacent ice tiles into larger rectangular regions. This significantly reduces the number of colliders compared to generating one collider per tile.

    The original implementation relied on multiple quadratic passes that compared tiles against one another when identifying and generating collider groups, causing editor freezes even on small levels.

    This version uses HashSet lookups for constant-time tile checks and tracks grouped tiles as they are processed, allowing each tile to be evaluated only once during grouping. In practice, generation time was reduced from several seconds to only a few milliseconds, making it fast enough to run automatically whenever the level tilemap is updated.

    The result is a seamless level-design workflow with minimal editor overhead, even on large levels.
*/

public class GenerateLevelIceColliders : EditorWindow
{
    const string ObjName = "Level/IceColliders";
    public static readonly Vector3 GridOffset = new(-0.5f, 0, 0.5f);
    static readonly string prefix = "GenerateLevelIceColliders";

    [MenuItem("Tools/Level/Generate/Ice Colliders")]
    public static void Main()
    {
        var startTime = System.DateTime.Now;

        LevelManager levelManager = LevelManager.TryGet(
            $"{prefix}: Couldn't Generate Ice Colliders. No LevelManager was found on the scene!");
        if (levelManager == null) return;

        // Find or create container gameobj
        GameObject containerObj = Utils.Dev.FindOrCreateGameObject(
            ObjName, g => g.layer = LayerMasks.Ice.Index);

        // Delete previous box colliders
        BoxCollider[] prevColliders = containerObj.GetComponents<BoxCollider>();
        foreach (BoxCollider collider in prevColliders) DestroyImmediate(collider);

        // Store all snow ground positions on a hash set for quick lookup
        HashSet<Vector3Int> snowPositions = new(
            GameObject.FindGameObjectsWithTag("SnowGround").Select(g => Vector3Int.FloorToInt(g.transform.position))
        );

        HashSet<Vector3Int> groupedTilesPositions = new();
        int collidersCount = 0;

        // Generate level matrix and collider groups
        LevelManager.IterateLevelTiles(levelManager, pos =>
        {
            Vector3Int position = new(pos.x, 0, pos.y);
            bool isOnIce = CheckIceAt(position, snowPositions);
            bool isGrouped = IsPositionGrouped(position, groupedTilesPositions);

            if (isOnIce && !isGrouped)
            {
                // Calculate collider group, create collider and mark grouped tiles

                ColliderGroup colliderGroup = new(position);
                colliderGroup.Calculate(
                    snowPositions,
                    groupedTilesPositions,
                    LevelManager.PaddedBounds.GetMaxX(levelManager.LevelLength),
                    LevelManager.PaddedBounds.GetMaxZ()
                );
                colliderGroup.GenerateCollider(containerObj);
                collidersCount++;
            }
        });

        if (collidersCount == 0)
        {
            DestroyImmediate(containerObj);
            Debug.Log($"{prefix}: No Ice detected on the level, No colliders generated.");
            return;
        }

        double timeTaken = (System.DateTime.Now - startTime).TotalMilliseconds;
        Debug.Log($"{prefix}: Generated {collidersCount} BoxColliders for Ice areas. Time taken: {timeTaken} milliseconds.");
        EditorSceneManager.MarkAllScenesDirty();
    }

    class ColliderGroup
    {
        public readonly Vector3 StartingPoint;
        public int cols = 0, rows = 0;

        public ColliderGroup(Vector3 startingPoint)
        {
            StartingPoint = startingPoint;
        }

        public void Calculate(
            HashSet<Vector3Int> snowPositions,
            HashSet<Vector3Int> groupedTilesPositions,
            int maxX,
            int maxZ
        )
        {
            var (x, y) = (0, 0);

            while (true)
            {
                List<Vector3Int> rowLevelPoints = new();

                while (true)
                {
                    var (nextX, nextY) = (StartingPoint.x + x, StartingPoint.z + y);
                    bool isOutOfBounds = nextX > maxX || nextY > maxZ;

                    if (!isOutOfBounds)
                    {
                        Vector3Int currentPosition = new((int)nextX, 0, (int)nextY);
                        bool isIce = CheckIceAt(currentPosition, snowPositions);
                        bool isGrouped = IsPositionGrouped(currentPosition, groupedTilesPositions);

                        if (isIce && !isGrouped)
                        {
                            rowLevelPoints.Add(currentPosition);
                            if (rows == 0) cols++;
                            if (x < cols) x++;

                            if (x < cols || rows == 0)
                            {
                                continue;
                            }
                        }
                    }

                    // On out of bounds or last point reached
                    if (x >= cols)
                    {
                        foreach (Vector3Int point in rowLevelPoints)
                            groupedTilesPositions.Add(point);

                        rows++;
                    }
                    else return;

                    x = 0;
                    break;
                }

                y++;

                // Security break
                if (x > maxX || y > maxZ) return;
            }
        }

        public void GenerateCollider(GameObject containerObj)
        {
            float x = StartingPoint.x + (float)cols / 2;
            float y = StartingPoint.z - 1 + (float)rows / 2;

            Vector3 size = new(cols, 1, rows);
            Vector3 center = new Vector3(x, 0, y) + GridOffset;

            BoxCollider collider = containerObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            collider.center = center;
        }
    }

    static bool CheckIceAt(Vector3Int position, HashSet<Vector3Int> snowPositions)
    {
        return !snowPositions.Contains(position);
    }

    static bool IsPositionGrouped(
        Vector3Int position,
        HashSet<Vector3Int> groupedTilesPositions
    )
    {
        return groupedTilesPositions.Contains(position);
    }
}
