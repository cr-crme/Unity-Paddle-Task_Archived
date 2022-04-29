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

	// Sets Target Line height based on HMD eye level and target position preference
	public void SetHeight(float offset)
	{
		Vector3 oldPosition = transform.position;

		float x = oldPosition.x;
		float z = oldPosition.z;
		float y = ComputeTargetHeight(GetHmdHeight()) + offset;

		transform.position = new Vector3(x, y, z);
	}

	public void SetWidth(float width)
	{
		transform.localScale = new Vector3(
			transform.localScale.x, 
			sessionManager.targetWidth * 2f,
			transform.localScale.z
		);
	}

	private float ComputeTargetHeight(float eyeLevel)
	{
		switch (sessionManager.targetHeight)
		{
			case SessionType.TargetHeight.RAISED:
				return eyeLevel * 1.1f;
			case SessionType.TargetHeight.LOWERED:
				return eyeLevel * 0.9f;
			case SessionType.TargetHeight.EYE_LEVEL:
				return eyeLevel;
			default:
				Debug.LogError("Error: Invalid Target Height Preference");
				return eyeLevel;
		}
	}

	private float GetHmdHeight()
	{
		return hmd.transform.position.y;
	}
}
