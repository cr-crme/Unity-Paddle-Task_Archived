using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private BallSoundManager ballSoundManager;
    private BallColorManager ballColorManager;

    [SerializeField, Tooltip("The target the ball should reach")]
    private Target target;

    [SerializeField, Tooltip("The trials manager")]
    private TrialsManager trialsManager;

    [SerializeField]
    private UiManager uiManager;

    // The current bounce effect in a forced exploration condition
    public Vector3 currentBounceModification;

    // If the ball just bounced, this will be true (momentarily)
    private bool justBounced = false;

    private bool _isCollidingWithPaddle;
    public bool shouldCollideWithPaddle
    {
        get { return _isCollidingWithPaddle; }
        set { _isCollidingWithPaddle = value; }
    }


    // A reference to this ball's rigidbody and collider
    private Kinematics kinematics;
    public EffectController effectController;
    private Rigidbody rigidBody;

    // Variables to keep track of resetting the ball after dropping to the ground
    public bool isOnGround { get { return transform.position.y < transform.localScale.y; } }
    public bool inHoverMode { get; protected set; } = true;
    public bool inRespawnMode { get; protected set; } = false;
    private int ballRespawnSeconds = 1;


    void Awake()
    {
        ballSoundManager = GetComponent<BallSoundManager>();
        ballColorManager = GetComponent<BallColorManager>();
        effectController = GetComponent<EffectController>();

        kinematics = GetComponent<Kinematics>();
        kinematics.storedPosition = SpawnPosition;

        rigidBody = GetComponent<Rigidbody>();
    }

    public void ForceToDrop()
    {
        shouldCollideWithPaddle = false;
    }

    public void TriggerPause()
    {
        kinematics.TriggerPause();
        shouldCollideWithPaddle = false;
    }

    public void TriggerResume()
    {
        if (inHoverMode) return;
        kinematics.TriggerResume();
        shouldCollideWithPaddle = true;
    }



    #region Collision with paddle
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
        BounceBall(paddleVelocity, cpNormal);
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
    #endregion



    #region Bounce logic
    private void BounceBall(Vector3 paddleVelocity, Vector3 cpNormal)
    {
        if (!shouldCollideWithPaddle)
            return;

        if (trialsManager.isPreparingNewTrial)
            // Deactivate contact with paddle when we are not in a trial
            return;

        // Manage Bounce Coroutine
        IEnumerator ManageBounceInTargetCoroutine()
        {
            // Wait until the ball reach the target then compute if the trial is valid
            // We wait even if there is no target as it prevents from double bounces
            while (!kinematics.ReachedApex()) { yield return null; }
            if (trialsManager.hasTarget && target.IsInsideTarget(kinematics.GetCurrentPosition()))
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

            StartCoroutine(ManageBounceInTargetCoroutine());
        }

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

    public void IndicateSuccessBall()
    {
        if (!trialsManager.hasTarget)
            return;

        // Turns ball green briefly and plays success sound.
        ballSoundManager.PlaySuccessSound();
        ballColorManager.IndicateSuccess();
    }
    #endregion



    #region Spawning manager
    public void ResetBall()
    {
        ResetBallProperties();
        trialsManager.StartNewTrial();
    }

    private void ResetBallProperties()
    {
        ballColorManager.SetToNormalColor();

        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        transform.position = SpawnPosition;
        transform.rotation = Quaternion.identity;
    }

    // Returns the default spawn position of the ball:
    // 10cm above the target line, 50cm in front of the 0 
    public Vector3 SpawnPosition
    {
        get { return new Vector3(0.0f, target.transform.position.y + 0.1f, 0.5f); }
    }

    public IEnumerator RespawningCoroutine(GlobalPauseHandler pauseHandler, bool spawnOnly = false)
    {
        if (!spawnOnly)
        {
            inRespawnMode = true;
            Time.timeScale = 1f; // Normal speed for the animation
            effectController.StopAllParticleEffects();
            effectController.StartVisualEffect(effectController.dissolve);
            yield return new WaitForSeconds(ballRespawnSeconds);
            yield return new WaitForEndOfFrame();
            effectController.StopParticleEffect(effectController.dissolve);
            inRespawnMode = false;
        }

        ManageHoveringPhaseCoroutine(pauseHandler);
    }
    #endregion


    #region Hovering manager
    // Holds the ball over the paddle at Target Height for 0.5 seconds, then releases
    private void ManageHoveringPhaseCoroutine(GlobalPauseHandler pauseHandler)
    {
        // Drops ball after reset
        IEnumerator ReleaseHoverAfterCountdown(float time)
        {
            yield return new WaitForSeconds(time);

            // Stop hovering
            inHoverMode = false;
            shouldCollideWithPaddle = true;
            pauseHandler.Resume();
        }

        IEnumerator PlayDropSoundCoroutine(float _waitTimeBeforePlaying)
        {
            yield return new WaitForSeconds(_waitTimeBeforePlaying);
            ballSoundManager.PlayDropSound();
        }

        inHoverMode = true;
        shouldCollideWithPaddle = false;

        ResetBallProperties();
        pauseHandler.Pause();

        effectController.StartVisualEffect(effectController.respawn);

        int resetTime = GlobalControl.Instance.ballResetHoverSeconds;

        // Hover ball at target line for a second
        StartCoroutine(PlayDropSoundCoroutine(resetTime - 0.15f));
        StartCoroutine(ReleaseHoverAfterCountdown(resetTime));

        // Show countdown to participant
        StartCoroutine(uiManager.ManageCountdownToDropCanvasCoroutine(resetTime));
    }

    #endregion


    #region Difficulty and level
    public void UpdatePhysics(DifficultyManager _difficultyManager)
    {
        kinematics.UpdateGravityMultiplyer(_difficultyManager.ballSpeed);
    }
    #endregion
}
