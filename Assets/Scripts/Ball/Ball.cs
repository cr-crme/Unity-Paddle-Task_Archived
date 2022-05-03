using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{

    [Tooltip("The script that handles the game logic")]
    [SerializeField]
    private PaddleGame gameScript;

    [SerializeField, Tooltip("Handles the ball sound effects")]
    private BallSoundManager ballSoundManager;

    [SerializeField, Tooltip("Handles the ball coloring")] 
    private BallColorManager ballColorManager;

    [SerializeField, Tooltip("The target the ball should reach")]
    private Target target;

    [SerializeField, Tooltip("The trials manager")]
    private TrialsManager trialsManager;

    // The current bounce effect in a forced exploration condition
    public Vector3 currentBounceModification;

    private SphereCollider sphereCollider;

    // If the ball just bounced, this will be true (momentarily)
    private bool justBounced = false;

    // A reference to this ball's rigidbody and collider
    private Kinematics kinematics; 



    // Variables to keep track of resetting the ball after dropping to the ground
    public bool inHoverMode { get; protected set; } = true;
    public bool inRespawnMode { get; protected set; } = false;
    private bool inHoverResetCoroutine = false;
    private bool inPlayDropSoundRoutine = false;
    private int ballResetHoverSeconds = 3;
    private int ballRespawnSeconds = 1;

    public EffectController effectController;

    void Awake()
    {
        kinematics = GetComponent<Kinematics>();
        effectController = GetComponent<EffectController>();

        sphereCollider = GetComponent<SphereCollider>();

        // Physics for ball is disabled until Space is pressed
        kinematics.TriggerPause();
    }

    void OnCollisionEnter(Collision c)
    {
        // On collision with paddle, ball should bounce
        if (c.gameObject.tag == "Paddle")
        {
            Paddle paddle = Paddle.GetPaddleFromCollider(c);
            Vector3 paddleVelocity = paddle.Velocity;
            Vector3 cpNormal = c.GetContact(0).normal;
            BounceBall(paddleVelocity, cpNormal);
        }
    }

    public void SimulateOnCollisionEnterWithPaddle(Vector3 paddleVelocity, Vector3 cpNormal)
    {
        if (IsCollisionEnabled)
        {
            BounceBall(paddleVelocity, cpNormal);
        }
    }

    // For every frame that the ball is still in collision with the paddle, 
    // apply a fraction of the paddle velocity to the ball. This is a fix for
    // the "sticky paddle" effect seen at low ball speeds. Crude approximation 
    // of acceleration. 
    void OnCollisionStay(Collision c)
    {
        if (c.gameObject.tag == "Paddle")
        {
            float pVySlice = Paddle.GetPaddleFromCollider(c).Velocity.y / 8.0f;    // 8 is a good divisor
            kinematics.AddToVelocity(new Vector3(0, pVySlice, 0));
        }
    }

    // Returns the default spawn position of the ball:
    // 10cm above the target line, 50cm in front of the 0 
    public Vector3 SpawnPosition
    {
        get { return new Vector3(0.0f, target.transform.position.y + 0.1f, 0.5f); }
    }

    public IEnumerator Respawning(GlobalPauseHandler pauseHandler)
    {
        inRespawnMode = true;
        Time.timeScale = 1f;
        effectController.StopAllParticleEffects();
        effectController.StartVisualEffect(effectController.dissolve);
        yield return new WaitForSeconds(ballRespawnSeconds);
        inRespawnMode = false;
        inHoverMode = true;
        yield return new WaitForEndOfFrame();
        effectController.StopParticleEffect(effectController.dissolve);
        pauseHandler.Pause();
        effectController.StartVisualEffect(effectController.respawn);
        yield return new WaitForSeconds(ballResetHoverSeconds);
        ballColorManager.SetToNormalColor();
    }

    private void BounceBall(Vector3 paddleVelocity, Vector3 cpNormal)
    {
        // Manage Bounce Coroutine
        IEnumerator ManageBounceInTarget()
        {
            bool isApexInTarget = true;  // Automatically accurate if there is no target
            if (trialsManager.hasTarget)
            {
                // Wait until the ball reach the target then compute if the trial is valid
                while (!kinematics.ReachedApex()) { yield return null; }
                isApexInTarget = target.IsInsideTarget(kinematics.GetCurrentPosition());
            }

            if (isApexInTarget)
            {
                IndicateSuccessBall();  // Flash ball green
                trialsManager.AddAccurateBounceToCurrentTrial();
            }

            // Setting justBounce here ensure that the ball starts decending before being available again
            justBounced = false;
        }

        // Determine if collision should be counted as an active bounce
        if (paddleVelocity.magnitude >= 0.05f && !justBounced)
        {
            justBounced = true;
            ballSoundManager.PlayBounceSound();
            trialsManager.AddBounceToCurrentTrial();
            effectController.SelectScoreDependentEffects(trialsManager.currentNumberOfBounces);
            kinematics.ApplyBouncePhysics(paddleVelocity, cpNormal, kinematics.storedVelocity);

            StartCoroutine(ManageBounceInTarget());
        }

    }

    public bool isOnGround()
    {
        return transform.position.y < transform.localScale.y;
    }


    public void IndicateSuccessBall()
    {
        // Turns ball green briefly and plays success sound.
        ballSoundManager.PlaySuccessSound();
        ballColorManager.IndicateSuccess();
    }

    // Ball has been reset. Reset the trial as well.
    public void ResetBall()
    {
        ballColorManager.SetToNormalColor();
        gameScript.ResetTrial();
    }

    // Drops ball after reset
    public IEnumerator ReleaseHoverOnReset(float time)
    {
        if (inHoverResetCoroutine)
        {
            yield break;
        }
        inHoverResetCoroutine = true;

        yield return new WaitForSeconds(time);

        // Stop hovering
        inHoverMode = false;
        inHoverResetCoroutine = false;
        inPlayDropSoundRoutine = false;

        IsCollisionEnabled = true;
    }

    public bool IsCollisionEnabled { 
        get { return sphereCollider.enabled; }
        set { sphereCollider.enabled = value; } 
    }

    // Play drop sound
    public IEnumerator PlayDropSound(float time)
    {
        if (inPlayDropSoundRoutine)
        {
            yield break;
        }
        inPlayDropSoundRoutine = true;
        yield return new WaitForSeconds(time);

        ballSoundManager.PlayDropSound();
    }

    

    // Modifies the bounce for this forced exploration game
    public void SetBounceModification(Vector3 modification)
    {
        currentBounceModification = modification;
    }

    // Returns the bounce for this forced exploration game
    public Vector3 GetBounceModification()
    {
        return currentBounceModification;
    }

}
