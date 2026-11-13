using UnityEngine;

public class LevelDollyCamera : LevelCamera
{
    void Start()
    {
        float posX = LevelManager.I.LevelLength - LevelCamerasManager.I.BoundsGapX;

        Vector3 position = transform.position;
        position.x = posX;
        transform.position = position;
    }
}
