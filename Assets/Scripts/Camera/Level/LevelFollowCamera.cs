using Unity.Cinemachine;
using UnityEngine;

public class LevelFollowCamera : LevelCamera
{
	[SerializeField] Transform guide;

	[Header("Movement")]
	[SerializeField] float moveRangeY = 0.5f;
	[SerializeField] float moveRangeZ = 0.9f;

	[Header("Rotation")]
	[SerializeField] float rotateRangeX = 1.5f;
	[SerializeField] float rotationSmoothTime = 1f;

	[Header("Look Ahead")]
	[SerializeField][Range(0, 7)] float lookAhead = 4.85f;
	[SerializeField] float lookAheadVelocity = 37f;
	[SerializeField][Range(0, 1)] float lookAheadCooldown = 0.3f;
	[SerializeField] float lookAheadPositionDamp = 3.3f;

	PlayerMovement playerMovement;

	float currentLookAhead = 0;
	float lookAheadCooldownRemaining;
	bool isLookingAhead = false;
	float initialSmoothDampX;
	CinemachineFollow cinemachineFollow;

	float playerMoveFactorZ = 0;
	float initialRotationX;
	float currentRotationVelocityXRef;

	readonly float MIN_PLAYER_SPEED_X = 0.75f;

	void Start()
	{
		playerMovement = Utils.GetComp<PlayerMovement>("Player");
		cinemachineFollow = GetComponent<CinemachineFollow>();

		initialSmoothDampX = cinemachineFollow.TrackerSettings.PositionDamping.x;
		lookAheadCooldownRemaining = lookAheadCooldown;
		initialRotationX = transform.eulerAngles.x;

		RefreshGuide();
	}

	void LateUpdate()
	{
		if (playerMovement)
		{
			playerMoveFactorZ = playerMovement.transform.position.z / (LevelManager.LevelDepth / 2);
			HandleLookAhead();

			HandleRotation();
			RefreshGuide();
		}
	}

	void RefreshGuide()
	{
		float newPositionX = playerMovement.transform.position.x + currentLookAhead;
		float newPositionZ = playerMoveFactorZ * moveRangeZ;
		float newPositionY = playerMoveFactorZ * moveRangeY;

		Vector3 newPosition = new(newPositionX, newPositionY, newPositionZ);
		newPosition = ClampWithBoundsX(newPosition);

		guide.transform.position = newPosition;
	}

	void HandleRotation()
	{
		Vector3 euler = transform.eulerAngles;
		float targetX = initialRotationX + playerMoveFactorZ * rotateRangeX;

		float smothedAngle = Mathf.SmoothDampAngle(
			current: euler.x,
			target: targetX,
			currentVelocity: ref currentRotationVelocityXRef,
			smoothTime: rotationSmoothTime
		);
		transform.rotation = Quaternion.Euler(smothedAngle, euler.y, euler.z);
	}

	void HandleLookAhead()
	{
		bool playerIsMoving = Mathf.Abs(playerMovement.Velocity.x) > MIN_PLAYER_SPEED_X;

		// Handle look ahead cooldown
		if (playerIsMoving && lookAheadCooldownRemaining > 0)
			lookAheadCooldownRemaining -= Time.deltaTime;
		else if (!playerIsMoving)
			lookAheadCooldownRemaining = lookAheadCooldown;

		isLookingAhead = playerIsMoving && lookAheadCooldownRemaining <= 0;

		// Set target
		float target = isLookingAhead
			? Mathf.Sign(playerMovement.Velocity.x) * lookAhead : 0;

		// Set a special position damping for looking ahead
		cinemachineFollow.TrackerSettings.PositionDamping.x = isLookingAhead
			? lookAheadPositionDamp : initialSmoothDampX;

		currentLookAhead = Mathf.MoveTowards(currentLookAhead, target, lookAheadVelocity * Time.deltaTime);
	}

	// Makes sure given vector is within level bounds
	Vector3 ClampWithBoundsX(Vector3 vector)
	{
		Vector2 boundsX_minmax = new(LevelCamerasManager.I.BoundsGapX, LevelManager.I.LevelLength - LevelCamerasManager.I.BoundsGapX);
		vector.x = Mathf.Clamp(vector.x, boundsX_minmax.x, boundsX_minmax.y);
		return vector;
	}
}
