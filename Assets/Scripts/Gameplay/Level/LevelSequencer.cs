using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class LevelSequencer : MonoBehaviour
{
    public static LevelSequencer I { get; private set; }
    public bool IsSequencing { get; private set; } = false;

    [Header("Doors")]
    [field: SerializeField] public LevelDoorEntrance EntranceDoor { get; private set; }
    [field: SerializeField] public LevelDoorExit ExitDoor { get; private set; }

    readonly float TRANSITION_WAIT_TIME = 0.33f;

    PlayerMovement playerMovement;
    PlayerController playerController;
    SledController sledController;
    public CharactersSequencer Characters { get; private set; }

    /// <summary>
    /// Event invoked when the entrance sequence finishes, after the player has moved to the starting position and controls are enabled.
    /// </summary>
    public static event Action OnFinishEntranceSequence;

    /// <summary>
    /// Event invoked at the start of the exit sequence, after player controls are disabled and before the player starts moving towards the exit door.
    /// </summary>
    public static event Action OnStartExitSequence;

    void Awake()
    {
        I = this;

        // Initialize Characters API
        Characters = new();

        // Get references to the PlayerMovement and PlayerController
        playerMovement = Utils.GetComp<PlayerMovement>("Player");
        playerController = playerMovement.GetComponent<PlayerController>();

        // Check if the sled is in the scene and get its controller reference
        GameObject sled = GameObject.FindWithTag("Sled");
        if (sled) sledController = sled.GetComponent<SledController>();
    }

    /// <summary>
    /// Handles sequencing of the player and sled at the start and end of the level, including teleporting to initial position, moving to the starting position, and moving towards the exit door. Also handles enabling and disabling player controls during sequences.
    /// </summary>
    public class CharactersSequencer
    {
        readonly float playerThrowSledThreshold = 0.5f;
        readonly float sledPushMoveTime = 0.75f;
        readonly float entranceExitDistanceFromDoor = 8;
        readonly float entranceFinalPlayerPosX = 1;
        readonly float entranceInitPlayerPosX;
        readonly float exitFinalPlayerPosX;
        readonly LevelSequencer I;

        public CharactersSequencer()
        {
            I = LevelSequencer.I;
            entranceInitPlayerPosX = entranceFinalPlayerPosX - entranceExitDistanceFromDoor;
            exitFinalPlayerPosX = entranceExitDistanceFromDoor + LevelManager.I.LevelLength;
        }

        /// <summary>Teleports both the player and the sled to their initial position all the way to the left.</summary>
        public void InitializePosition()
        {
            Vector3 playerInitialPosition = new(entranceInitPlayerPosX, 1, 0);
            Vector3 sledInitialPosition = playerInitialPosition + Vector3.right;

            I.playerMovement.transform.position = playerInitialPosition;
            I.playerMovement.SnapPosition();

            if (I.sledController)
                I.sledController.transform.position = sledInitialPosition;

            I.playerMovement.DisableMovement();
            I.playerController.DisableInteraction();
        }

        /// <summary>Moves both the player and the sled to the starting gameplay position.</summary>
        void ToGameplayStartingPosition()
        {
            if (I.sledController)
                I.sledController.ForceStartCarrying();

            Vector3 finalPlayerPosition = new(entranceFinalPlayerPosX, 1, 0);
            I.playerMovement.MoveTowards(finalPlayerPosition, true);
        }

        public void StartEntranceSequence()
        {
            I.IsSequencing = true;
            I.StartCoroutine(EntranceSequence());
        }

        IEnumerator EntranceSequence()
        {
            // Move player and sled to their initial gameplay position
            ToGameplayStartingPosition();

            // Close the entrance door after player and sled pass through
            yield return new WaitUntil(() =>
                I.playerMovement.transform.position.x >= entranceFinalPlayerPosX - playerThrowSledThreshold);
            I.EntranceDoor.Close();

            if (I.sledController)
            {
                // Make player release the Sled and push it to the right
                I.sledController.ForceStopCarrying();
                Vector3 sledTargetPosition = Vector3.right * (entranceFinalPlayerPosX + 2);
                I.sledController.MoveTowards(sledTargetPosition, sledPushMoveTime, Ease.OutSine);
            }

            // Wait until player stops moving and enable controls
            yield return new WaitUntil(() => !I.playerMovement.IsMoving);
            I.playerController.EnableInteraction();
            I.playerMovement.EnableMovement();
            I.EntranceDoor.Close();

            I.IsSequencing = false;
            OnFinishEntranceSequence?.Invoke();
        }

        public void StartExitSequence()
        {
            if (!LevelManager.I.HasFinished)
            {
                OnStartExitSequence?.Invoke();
                I.StartCoroutine(ExitSequence());
            }
        }

        IEnumerator ExitSequence()
        {
            I.IsSequencing = true;

            // Disable player controls
            I.playerController.DisableInteraction();
            I.playerMovement.DisableMovement();
            PauseMenuManager.I.DisablePausing();

            if (I.sledController)
                I.sledController.ForceStartCarrying();

            // Move off the screen to the right side 
            I.playerMovement.MoveTowards(Vector3.right * exitFinalPlayerPosX);

            // Wait before transitioning to the next level
            yield return new WaitForSeconds(I.TRANSITION_WAIT_TIME);

            LoadNextlevel();
        }

        void LoadNextlevel()
        {
            bool thereIsANextLevel = LevelManager.LevelScene.GetNextIndex() != -1;

            if (thereIsANextLevel)
            {
                TransitionManager.I.Towards(SceneTarget.NextLevel)
                    .SetTransition(Transition.SnowFlakes)
                    .OnTransitionInComplete(() => I.IsSequencing = false)
                    .OnEarlyNewSceneLoaded(() => PauseMenuManager.I.DisablePausing())
                    .OnNewSceneLoaded(() => LevelSequencer.I?.Characters.InitializePosition())
                    .OnTransitionOutComplete(() =>
                    {
                        LevelSequencer.I?.Characters.StartEntranceSequence();
                        PauseMenuManager.I?.EnablePausing();
                    });

                return;
            }

            TransitionManager.I.Towards(Scenes.UI.MainMenu.Index)
                .SetTransition(Transition.WhiteFade)
                .OnTransitionInComplete(() =>
                {
                    I.IsSequencing = false;
                    GameManager.I.Global.CurrentLevel = 1;
                });
        }
    }

    // Reset static instance on destroy
    void OnDestroy() => I = null;
}
