using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
	[Tooltip("The head mounted display")]
	[SerializeField]
	private Camera hmd;

	[SerializeField]
	private SessionManager sessionManager;

    private void Start()
    {
		UpdateCondition();
	}

    // Sets Target Line height based on HMD eye level and target position preference
	public void UpdateCondition()
    {
		UpdateHeight();
		UpdateWidth();
		UpdateToggle();
	}

    private void UpdateHeight()
	{
		Vector3 oldPosition = transform.position;

		float x = oldPosition.x;
		float z = oldPosition.z;
		float y = ComputeTargetHeight(GetHmdHeight()) + sessionManager.targetHeightOffset;

		transform.position = new Vector3(x, y, z);
	}

	private void UpdateWidth()
	{
		transform.localScale = new Vector3(
			transform.localScale.x, 
			sessionManager.targetWidth * 2f,
			transform.localScale.z
		);
	}
	private void UpdateToggle()
	{
		gameObject.SetActive(sessionManager.hasTarget);
	}

	private float ComputeTargetHeight(float eyeLevel)
	{
		switch (sessionManager.targetHeight)
		{
			case TargetEnum.Height.RAISED:
				return eyeLevel * 1.1f;
			case TargetEnum.Height.LOWERED:
				return eyeLevel * 0.9f;
			case TargetEnum.Height.EYE_LEVEL:
				return eyeLevel;
			default:
				Debug.LogError("Error: Invalid Target Height Preference");
				return eyeLevel;
		}
	}

	// Returns true if the ball is within the target line boundaries.
	public bool IsInsideTarget(Vector3 height)
	{
		if (!sessionManager.hasTarget) return false;

		float targetHeight = transform.position.y;
		float lowerLimit = targetHeight - sessionManager.targetWidth;
		float upperLimit = targetHeight + sessionManager.targetWidth;

		return (height.y > lowerLimit) && (height.y < upperLimit);
	}

	private float GetHmdHeight()
	{
		return hmd.transform.position.y;
	}
}
