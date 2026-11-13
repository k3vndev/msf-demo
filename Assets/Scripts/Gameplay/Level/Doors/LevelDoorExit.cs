using UnityEngine;

public class LevelDoorExit : LevelDoor
{
    readonly float MIN_PLAYER_DISTANCE_ENTER = 5.5f;
    readonly float MIN_PLAYER_DISTANCE_GONE = 1.5f;

    protected override void Update()
    {
        base.Update();

        float playerAngle = Quaternion.Angle(PlayerMovement.MeshModel.rotation, Quaternion.Euler(Vector3.zero));
        float playerDistance = Utils.Vectors.DistanceIgnoreY(transform.position, PlayerMovement.transform.position);

        bool playerIsFacingDoor = playerAngle < Consts.MIN_ROTATION_DEG && playerDistance < MIN_PLAYER_DISTANCE_ENTER
            && PlayerMovement.transform.position.x - transform.position.x < MIN_PLAYER_DISTANCE_GONE;

        if (SledController.IsBeingCarried && playerIsFacingDoor)
        {
            Open();
            return;
        }
        Close();
    }

    protected override void OnSledTriggerEnter()
    {
        LevelSequencer.I.Characters.StartExitSequence();
    }
}
