using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Tooltip("The normal ball color")]
    [SerializeField]
    private Material ballMat;

    [Tooltip("The ball color when it is in the target height range")]
    [SerializeField]
    private Material greenBallMat;

    [Tooltip("Auxilliary color materials")]
    [SerializeField]
    private Material redBallMat;
    [SerializeField]
    private Material blueBallMat;

    [Tooltip("The script that handles the game logic")]
    [SerializeField]
    private PaddleGame gameScript;

    [SerializeField, Tooltip("Handles the ball sound effects")]
    private BallSoundPlayer ballSoundPlayer;

    // The current bounce effect in a forced exploration condition
    public Vector3 currentBounceModification;

    // This is true when the player is currently paddling the ball. If the 
    // player stops paddling the ball, set to false.
    public bool isBouncing = false;

    // If the ball just bounced, this will be true (momentarily)
    private bool justBounced = false;

    // A reference to this ball's rigidbody and collider
    private Kinematics kinematics; 

    // For Green/White IEnumerator coroutine 
    bool inTurnBallWhiteCR = false;


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
        ballSoundPlayer = GetComponent<BallSoundPlayer>();
        effectController = GetComponent<EffectController>();

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
        // if ball collides with the floor or something random, it is no longer bouncing
        else
        {
            isBouncing = false;
        }
    }

    public void SimulateOnCollisionEnterWithPaddle(Vector3 paddleVelocity, Vector3 cpNormal)
    {
        if (GetComponent<SphereCollider>().enabled)
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

    // Returns the default spawn position of the ball (10cm above the target line) 
    public static Vector3 spawnPosition(GameObject targetLine)
    {
        return new Vector3(0.0f, targetLine.transform.position.y + 0.1f, 0.5f);
    }

    public IEnumerator Respawning(GlobalPauseHandler pauseHandler)
    {
        Debug.Log("Respawning started " + Time.timeScale);
        inRespawnMode = true;
        Time.timeScale = 1f;
        effectController.StopAllParticleEffects();
        effectController.StartEffect(effectController.dissolve);
        yield return new WaitForSeconds(ballRespawnSeconds);
        inRespawnMode = false;
        inHoverMode = true;
        yield return new WaitForEndOfFrame();
        effectController.StopParticleEffect(effectController.dissolve);
        pauseHandler.Pause();
        effectController.StartEffect(effectController.respawn);
        yield return new WaitForSeconds(ballRespawnSeconds);
        TurnBallWhite();
        Debug.Log("Respawning finished " + Time.timeScale);
    }

    private void BounceBall(Vector3 paddleVelocity, Vector3 cpNormal)
    {
        Vector3 Vin = kinematics.storedVelocity;
        
        kinematics.ApplyBouncePhysics(paddleVelocity, cpNormal, Vin);

        // Determine if collision should be counted as an active bounce
        if (paddleVelocity.magnitude < 0.05f)
        {
            isBouncing = false;
        }
        else
        {
            isBouncing = true;

            CheckApexSuccess();
            DeclareBounce();
            ballSoundPlayer.PlayBounceSound();
        }
    }

    public bool isOnGround()
    {
        return transform.position.y < transform.localScale.y;
    }

    void CheckApexSuccess()
    {
        StartCoroutine(CheckForApex());
    }

    IEnumerator CheckForApex()
    {
        yield return new WaitWhile( () => !kinematics.ReachedApex());

        float apexHeight = kinematics.GetCurrentHeight();
        bool successfulBounce = gameScript.GetHeightInsideTargetWindow(apexHeight);

        if (successfulBounce) { 
            IndicateSuccessBall();       // Flash ball green 
        }
    }

    // Turns ball green briefly and plays success sound.
    public void IndicateSuccessBall()
    {
        ballSoundPlayer.PlaySuccessSound();

        TurnBallGreen();
        StartCoroutine(TurnBallWhiteCR(0.3f));
    }

    

    // Try to declare that the ball has been bounced. If the ball
    // was bounced too recently, then this declaration will fail.
    // This is to ensure that bounces are only counted once.
    public void DeclareBounce()
    {
        if (justBounced)
        {
            // do nothing, this bounce has already been counted
            return;
        }
        else
        {
            justBounced = true;
            gameScript.BallBounced();
            StartCoroutine(FinishBounceDeclaration());
        }
    }

    // Wait a little bit before a bounce can be declared again.
    // This is to ensure that bounces are not counted multiple times.
    IEnumerator FinishBounceDeclaration()
    {
        yield return new WaitForSeconds(0.2f);
        justBounced = false;
    }

    // Ball has been reset. Reset the trial as well.
    public void ResetBall()
    {
        TurnBallWhite();
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

        GetComponent<SphereCollider>().enabled = true;
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

        ballSoundPlayer.PlayDropSound();
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

    public void TurnBallGreen()
    {
        GetComponent<MeshRenderer>().material = greenBallMat;
    }

    public void TurnBallRed()
    {
        GetComponent<MeshRenderer>().material = redBallMat;
    }

    public void TurnBallBlue()
    {
        GetComponent<MeshRenderer>().material = blueBallMat;
    }

    public void TurnBallWhite()
    {
        GetComponent<MeshRenderer>().material = ballMat;
    }

    public IEnumerator TurnBallWhiteCR(float time = 0.0f)
    {
        if (inTurnBallWhiteCR)
        {
            yield break;
        }
        yield return new WaitForSeconds(time);
        inTurnBallWhiteCR = true;

        TurnBallWhite();

        inTurnBallWhiteCR = false;
    }

    public IEnumerator TurnBallGreenCR(float time = 0.0f)
    {
        yield return new WaitForSeconds(time);
        TurnBallGreen();
    }
}
