using UnityEngine;

/// <summary>
/// This script extends the physics engine to allow for kinematic calculations.
/// It handles all the motion for the rigidbody this script is attached to
/// except for its useGravity property.
/// 
/// This script must be used in conjunction with a RigidBody and Collider. 
/// </summary>

public class Kinematics : MonoBehaviour
{
    // Reference to this GameObject's rigidbody
    [SerializeField] private Rigidbody rigidBody;
    
    public Vector3 storedPosition;
    public Vector3 storedVelocity;
    public Vector3 storedAngularVelocity;
    public Quaternion storedRotation;

    public CircularBuffer<Vector3> velocityBuffer;
    const int CIRCULAR_BUFFER_SIZE = 50;

    // Start is called before the first frame update
    void Awake()
    {
        storedPosition = rigidBody.position;
        storedVelocity = rigidBody.velocity;
        storedAngularVelocity = rigidBody.angularVelocity;
        storedRotation = Quaternion.identity;

        velocityBuffer = new CircularBuffer<Vector3>(CIRCULAR_BUFFER_SIZE);
    }

    // Handle physics
    void FixedUpdate()
    {
        // Handle Paused and Playing states separately.
        if (GlobalPauseHandler.Instance.isPaused)
        {
            DisableMotion();
        }
        else 
        {
            EnableMotion();
            StreamVelocityToBuffer();
        }
    }

    public Vector3 GetCurrentPosition()
    {
        return rigidBody.position;
    }

    public void AddToVelocity(Vector3 vector)
    {
        rigidBody.velocity += vector;
    }

    // Contains all the procedures to resume physics. Fires once per input.
    public void TriggerResume()
    {
        // Resume motion
        rigidBody.useGravity = true;
        rigidBody.velocity        = storedVelocity;
        rigidBody.angularVelocity = storedAngularVelocity;
        rigidBody.rotation        = storedRotation;

        // Allow collisions
        rigidBody.detectCollisions = true;
    }

    // Contains all the procedures to pause physics. Fires once per input.
    // TODO turn this into end-of-frame coroutine to avoid "teleporting" pause
    public void TriggerPause()
    {
        // Eliminate forces
        rigidBody.useGravity = false;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.rotation = Quaternion.identity;

        // Do not collide
        rigidBody.detectCollisions = false;
    }

    // Handles the paused physics state. Holds everything constant.
    void DisableMotion()
    {
        // Hold rb still
        rigidBody.position = storedPosition;
        rigidBody.velocity = storedVelocity;
        rigidBody.angularVelocity = storedAngularVelocity;
        rigidBody.rotation = storedRotation;
    }

    // Handles the enabled (regular) physics state.
    void EnableMotion()
    {
        // Update stored variables
        storedPosition = rigidBody.position;
        storedVelocity = rigidBody.velocity;
        storedAngularVelocity = rigidBody.angularVelocity;
        storedRotation = rigidBody.rotation;
    }

    // Returns the reflected velocity vector of a given input vector and normal contact point.
    // Efficiency represents the percentage (0.0-1.0) of energy dissipated from the impact.  
    public Vector3 GetReflectionDamped(Vector3 inVector, Vector3 inNormal, float efficiency = 1.0f)
    {
        Vector3 reflected = Vector3.Reflect(inVector, inNormal);
        reflected *= efficiency;
        return reflected;
    }

    // Add position to a circular buffer of size 10
    private void StreamVelocityToBuffer()
    {
        velocityBuffer.Add(rigidBody.velocity);
    }

    // Returns true if the ball was previously going up and is now falling. 
    public bool ReachedApex()
    {
        Vector3[] buffer = velocityBuffer.GetArray();
        if (buffer.Length < CIRCULAR_BUFFER_SIZE)
        {
            return false;
        }

        // Apex is reached if velocity crosses from positive to negative
        bool ConsecPositiveVel = true;
        bool ConsecNegativeVel = true;

        // &= to ensure zero-crossing in buffer
        for (int i = 0; i < 5; i++)
        {
            ConsecPositiveVel &= (buffer[i].y > 0);
        }     
        for (int i = (CIRCULAR_BUFFER_SIZE - 3); i < (CIRCULAR_BUFFER_SIZE - 1); i++)
        {
            ConsecNegativeVel &= (buffer[i].y < 0);
        }
        
        return (ConsecPositiveVel && ConsecNegativeVel);
    }

    // Perform physics calculations to bounce ball. Includes ExplorationMode modifications.
    public void ApplyBouncePhysics(Vector3 paddleVelocity, Vector3 cpNormal, Vector3 Vin)
    {
        // Reduce the effects of the paddle tilt so the ball doesn't bounce everywhere
        Vector3 reducedNormal = ProvideLeewayFromUp(cpNormal);

        // Get reflected bounce, with energy transfer
        Vector3 Vreflected = GetReflectionDamped(Vin, reducedNormal, 0.8f);
        //TODO: if (...reduce energy transfer)
        //    Vreflected = LimitDeviationFromUp(Vreflected, GlobalControl.Instance.degreesOfFreedom);

        // Apply paddle velocity
        //TODO: if (... reduce velocity transfer)
        //    Vreflected = new Vector3(0, Vreflected.y + (1.25f * paddleVelocity.y), 0);
        Vreflected += new Vector3(0, paddleVelocity.y, 0);

        rigidBody.velocity = Vreflected;
    }

    private Vector3 ProvideLeewayFromUp(Vector3 n)
    {
        float degreesOfTilt = Vector3.Angle(Vector3.up, n);
        float limit = 2.0f; // feels pretty realistic through testing
        if (degreesOfTilt < limit)
        {
            degreesOfTilt /= limit;
        }
        else
        {
            degreesOfTilt -= limit;
        }
        return LimitDeviationFromUp(n, degreesOfTilt);
    }

    // If in Reduced condition, returns the vector of the same original magnitude and same x-z direction
    // but with adjusted height so that the angle does not exceed the desired degrees of freedom
    private Vector3 LimitDeviationFromUp(Vector3 v, float degreesOfFreedom)
    {
        if (Vector3.Angle(Vector3.up, v) <= degreesOfFreedom)
        {
            return v;
        }

        float bounceMagnitude = v.magnitude;
        float yReduced = bounceMagnitude * Mathf.Cos(degreesOfFreedom * Mathf.Deg2Rad);
        float xzReducedMagnitude = bounceMagnitude * Mathf.Sin(degreesOfFreedom * Mathf.Deg2Rad);
        Vector3 xzReduced = new Vector3(v.x, 0, v.z).normalized * xzReducedMagnitude;

        Vector3 modifiedBounceVelocity = new Vector3(xzReduced.x, yReduced, xzReduced.z);
        return modifiedBounceVelocity;
    }
}
