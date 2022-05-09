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
    private Rigidbody rigidBody;
    
    public Vector3 storedPosition;
    public Vector3 storedVelocity;
    public Vector3 storedAngularVelocity;
    public Quaternion storedRotation;

    public CircularBuffer<Vector3> velocityBuffer;
    const int CIRCULAR_BUFFER_SIZE = 15;

    private Vector3 initialGravity = Physics.gravity;

    // Start is called before the first frame update
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
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

    public Vector3 currentPosition { get { return rigidBody.position; } }

    public void AddToVelocity(Vector3 vector)
    {
        rigidBody.velocity += vector;
    }
    public Vector3 currentVelocity { get { return rigidBody.velocity; } }

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
    public void ApplyBouncePhysics(Vector3 _paddleVelocity, Vector3 _paddleNormal, Vector3 _ballVelocity)
    {
        Vector3 LimitDeviationFromUp(Vector3 _normal)
        {
            // If in Reduced condition, returns the vector of the same original magnitude and same x-z direction
            // but with adjusted height so that the angle does not exceed the desired degrees of freedom

            float ProvideLeewayFromUp(Vector3 _vector)
            {
                float _limit = 2.0f; // feels pretty realistic through testing
                float _angleToUp = Vector3.Angle(Vector3.up, _vector);
                return _angleToUp < _limit ? _angleToUp / _limit : _angleToUp - _limit;
            }

            float _tiltInDegree = ProvideLeewayFromUp(_normal);

            if (Vector3.Angle(Vector3.up, _normal) <= _tiltInDegree)
            {
                // If the angle to vertical is already small
                return _normal;
            }

            float _bounceMagnitude = _normal.magnitude;
            float _yReduced = _bounceMagnitude * Mathf.Cos(_tiltInDegree * Mathf.Deg2Rad);
            float _xzReducedMagnitude = _bounceMagnitude * Mathf.Sin(_tiltInDegree * Mathf.Deg2Rad);
            Vector3 _xzReduced = new Vector3(_normal.x, 0, _normal.z).normalized * _xzReducedMagnitude;

            Vector3 _modifiedBounceVelocity = new Vector3(_xzReduced.x, _yReduced, _xzReduced.z);
            return _modifiedBounceVelocity;
        }

        Vector3 GetReflectionDamped(Vector3 _inVector, Vector3 _inNormal, float _efficiency = 1.0f)
        {
            // Returns the reflected velocity vector of a given input vector and normal contact point.
            // Efficiency represents the percentage (0.0-1.0) of energy dissipated from the impact.  
            Vector3 _reflected = Vector3.Reflect(_inVector, _inNormal);
            _reflected *= _efficiency;
            return _reflected;
        }


        // Reduce the effects of the paddle tilt so the ball doesn't bounce everywhere
        Vector3 _velocityWithReducedAngleToUp = LimitDeviationFromUp(_paddleNormal);

        // Get reflected bounce, with energy transfer
        Vector3 _velocityOfReflectedBall = GetReflectionDamped(_ballVelocity, _velocityWithReducedAngleToUp, 0.8f);

        // Apply paddle velocity
        Vector3 _velocityAddedWithPaddle = _velocityOfReflectedBall + new Vector3(0, _paddleVelocity.y, 0);

        // Put the value in the rigid body 
        rigidBody.velocity = _velocityAddedWithPaddle;
    }




    public void UpdateGravityMultiplyer(double _scalar)
    {
        Physics.gravity = (float)_scalar * initialGravity;
    }
}
