using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Tooltip("The head mounted display")]
    [SerializeField]
    private Camera hmd;


    // Sets Target Line height based on HMD eye level and target position preference
    public void UpdateCondition(TrialsManager _trialsManager)
    {
        UpdateHeight(_trialsManager.targetBaseHeight, _trialsManager.targetHeightOffset);
        UpdateWidth(_trialsManager.targetWidth);
        UpdateToggle(_trialsManager.hasTarget);
    }

    private void UpdateHeight(TargetEnum.Height baseHeight, double _heightOffset)
    {
        Vector3 oldPosition = transform.position;

        float x = oldPosition.x;
        float z = oldPosition.z;
        float y = GetHmdHeight() * TargetBaseHeightModifier(baseHeight) + (float)_heightOffset;

        transform.position = new Vector3(x, y, z);
    }

    private void UpdateWidth(double _width)
    {
        transform.localScale = new Vector3(
            transform.localScale.x, (float)_width, transform.localScale.z
        );
    }
    private void UpdateToggle(bool _hasTarget)
    {
        gameObject.SetActive(_hasTarget);
    }

    private float TargetBaseHeightModifier(TargetEnum.Height baseHeight)
    {
        switch (baseHeight)
        {
            case TargetEnum.Height.RAISED:
                return 1.1f;
            case TargetEnum.Height.LOWERED:
                return 0.9f;
            case TargetEnum.Height.EYE_LEVEL:
                return 1f;
            default:
                Debug.LogError("Error: Invalid Target Height Preference");
                return 1f;
        }
    }

    // Returns true if the ball is within the target line boundaries.
    public bool IsInsideTarget(Vector3 height)
    {
        float targetHeight = transform.position.y;
        float lowerLimit = targetHeight - (transform.localScale.y / 2f);
        float upperLimit = targetHeight + (transform.localScale.y / 2f);

        return (height.y > lowerLimit) && (height.y < upperLimit);
    }

    private float GetHmdHeight()
    {
        return hmd.transform.position.y;
    }
}
