using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class PaddleGame : MonoBehaviour
{	
    private bool _isInTrial;

    // Manage the current task to perform
    [SerializeField, Tooltip("The main manager for the game difficulty")]
    private DifficultyManager difficultyManager;

    // Manage the current task to perform
    [SerializeField, Tooltip("The main trial manager for the game")]
    private TrialsManager trialsManager;
    
    [Tooltip("The ball being bounced")]
    [SerializeField]
    private GameObject ball;

    [Tooltip("The line that denotes where the ball should be bounced ideally")]
    [SerializeField]
    private Target targetLine;

    [Tooltip("The canvas that displays score information to the user")]
    [SerializeField]
    private FeedbackCanvas feedbackCanvas;

    [Tooltip("A reference to the Time to Drop countdown display quad")]
    [SerializeField]
    private GameObject timeToDropQuad;

    [Tooltip("A reference to the Time to Drop countdown display text")]
    [SerializeField]
    private Text timeToDropText;

    [SerializeField]
    private AudioSource difficultySource;

    [SerializeField]
    private TextMeshPro difficultyDisplay;

    /// <summary>
    /// list of the audio clips played at the beginning of difficulties in some cases
    /// </summary>
    [SerializeField]
    List<DifficultyAudioClip> difficultyAudioClips = new List<DifficultyAudioClip>();

    // The current trial number. This is increased by one every time the ball is reset.
    public int trialNum = 0;

    [SerializeField]
    private GlobalPauseHandler pauseHandler;

    // Degrees of freedom, how many degrees in x-z directions ball can bounce after hitting paddle
    // 0 degrees: ball can only bounce in y direction, 90 degrees: no reduction in range
    public float degreesOfFreedom;

    // Variables for countdown timer display
    private bool inCoutdownCoroutine = false;

    
    private int difficultyEvaluationIndex = 0;

    float difficultyExampleTime = 30f;

    void Start()
    {
        Instantiate(GlobalControl.Instance.environments[GlobalControl.Instance.environmentIndex]);

        // Calibrate the target line to be at the player's eye level
        if(GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
        {
            GlobalControl.Instance.practiseMaxTrialTime = 0;
        }

        SetTrialLevel(GlobalControl.Instance.level);
        Initialize(true);


        // difficulty shifts timescale, so pause it again
        Time.timeScale = 0;
        pauseHandler.Pause();
    }

    void Update()
    {
        if(GlobalControl.Instance.paused)
        {
            // no processing until unpaused
            return;
        }

        // Update Canvas display
        timeToDropQuad.SetActive(false);

        // Reset time scale
        Time.timeScale = GlobalControl.Instance.timescale;

        // Reset ball if it drops 
        ManageIfBallOnGround();
        ManageHoveringPhase();
    }

    void OnApplicationQuit()
    {
        QuitTask();
    }

    /// <summary>
    /// Stop the task, write data and return to the start screen
    /// </summary>
    public void QuitTask()
    {
        IEnumerator QuitWhenTrialIsProcessed()
        {
            yield return new WaitUntil(() => (trialsManager.isPreparingNewTrial));

            // clean DDoL objects and return to the start scene
            Destroy(GlobalControl.Instance.gameObject);
            Destroy(gameObject);

            SceneManager.LoadScene(0);
        }

        // This is to ensure that the final trial is recorded.
        trialsManager.ForceEndOfTrial();
        StartCoroutine(QuitWhenTrialIsProcessed());
    }

#region Initialization

    public void Initialize(bool firstTime)
    {
        if (GlobalControl.Instance.playVideo)
        {
            // Wait for end of video playback to initialize
            return;
        }
        trialsManager.StartNewTrial();

        // Initialize Condition and Visit types
        degreesOfFreedom = GlobalControl.Instance.degreesOfFreedom;


        if (GlobalControl.Instance.session == SessionType.Session.PRACTISE)
        {
            difficultyDisplay.text = difficultyManager.currentLevel.ToString();
        }
        else if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
        {
            difficultyManager.currentLevel = 2;
            StartShowcase();
        }
        else
        {
            Debug.LogError($"SessionType: {GlobalControl.Instance.session} not implemented yet");
        }

        feedbackCanvas.UpdateAllInformation(trialsManager);

        // ensure drop time on first drop
        if (firstTime)
        {
            ball.GetComponent<EffectController>().StopAllParticleEffects();
        }
        else
        {
            StartCoroutine(ball.GetComponent<Ball>().RespawningCoroutine(pauseHandler));
        }

        Debug.Log("Initialized");
    }



    /// <summary>
    /// run through all diffiuclties in a short amount of time to get a feel for them
    /// </summary>
    void StartShowcase()
    {
        pauseHandler.Resume();
        SetTrialLevel(difficultyManager.currentLevel);
        StartCoroutine(StartDifficultyDelayedCoroutine(difficultyExampleTime, true));
    }

    IEnumerator StartDifficultyDelayedCoroutine(float delay, bool initial = false)
    {
        if (initial)
        {
            // wait until after the pause is lifted, when timescale is 0
            yield return new WaitForSeconds(.1f);
        }

        var audioClip = GetDifficultyAudioClip(difficultyManager.currentLevel);
        if (audioClip != null)
        {
            difficultySource.PlayOneShot(audioClip);
        }
        Debug.Log("playing difficulty audio " + (audioClip != null ? audioClip.name : "null"));

        yield return new WaitForSecondsRealtime(delay);

        // reset ball, change difficulty level, possible audio announcement.
        if (difficultyManager.currentLevel >= 10)
        {
            // finish up the difficulty showcase, quit application
            QuitTask();
        }
        else
        {
            SetTrialLevel(difficultyManager.currentLevel + 2);
            StartCoroutine(StartDifficultyDelayedCoroutine(difficultyExampleTime));
            if (difficultyManager.currentLevel > 10) // OG ==
            {
                // yield return new WaitForSecondsRealtime(delay);
            }
        }

        ball.GetComponent<Ball>().ResetBall();
    }

#endregion // Initialization

#region Reset Trial

    // Holds the ball over the paddle at Target Height for 0.5 seconds, then releases
    void ManageHoveringPhase()
    {
        if (!ball.GetComponent<Ball>().inHoverMode)
            return;

        timeToDropQuad.SetActive(true);

        ball.GetComponent<Ball>().IsCollisionEnabled = false;

        // Hover ball at target line for a second
        StartCoroutine(ball.GetComponent<Ball>().PlayDropSoundCoroutine(GlobalControl.Instance.ballResetHoverSeconds - 0.15f));
        StartCoroutine(ball.GetComponent<Ball>().ReleaseHoverOnResetCoroutine(GlobalControl.Instance.ballResetHoverSeconds));

        // Start countdown timer 
        StartCoroutine(UpdateTimeToDropDisplayCoroutine());

        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        ball.transform.position = ball.GetComponent<Ball>().SpawnPosition;
        ball.transform.rotation = Quaternion.identity;

        Time.timeScale = 1f;
        //Debug.Log("Entering hover mode");
    }

    void ManageIfBallOnGround()
    {
        if (ball.GetComponent<Ball>().inHoverMode) 
            return;

        if (ball.GetComponent<Ball>().inRespawnMode)
            return;         
        
        // Check if ball is on ground
        if (ball.GetComponent<Ball>().isOnGround())
        {
            ResetTrial();
            _isInTrial = true;
        }
    }


    // Update time to drop
    IEnumerator UpdateTimeToDropDisplayCoroutine()
    {
        if (inCoutdownCoroutine)
        {
            yield break;
        }
        inCoutdownCoroutine = true;

        int countdown = GlobalControl.Instance.ballResetHoverSeconds;

        while (countdown >= 1.0f)
        {
            timeToDropText.text = countdown.ToString();
            countdown--;
            yield return new WaitForSeconds(1.0f);
        }

        inCoutdownCoroutine = false;
    }

    // The ball was reset after hitting the ground. Reset bounce and score.
    public void ResetTrial(bool final = false)
    {
        // Don't run this code the first time the ball is reset or when there are 0 bounces
        if (trialNum < 1 /*|| numBounces < 1*/)
        {
            trialNum++;
            return;
        }

        if (!_isInTrial)
            return;

        _isInTrial = false;

        if (!final && trialNum != 0 && trialNum % 10 == 0)
        {
            // some difficulty effects are regenerated every 10 trials
            SetTrialLevel(difficultyManager.currentLevel);
        }
            


        feedbackCanvas.UpdateBestSoFar(trialsManager.bestSoFarNbOfBounces);

        trialNum++;
        trialsManager.StartNewTrial();

        if (!final)
        {
            // Check if game should end or evaluation set change
            if (trialsManager.isSessionOver)
            {
                QuitTask();
                return;
            }
            Initialize(false);
        }
    }

#endregion // Reset

#region Checks, Interactions, Data

    private AudioClip GetDifficultyAudioClip(int difficulty)
    {
        foreach(var difficultyAudioClip in difficultyAudioClips)
        {
            if(difficultyAudioClip.difficulty == difficulty)
            {
                return difficultyAudioClip.audioClip;
            }
        }
        return null;
    }

    #endregion // Checks, Interactions, Data

    #region Difficulty
    void EvaluatePerformance()
    {
        double _score = trialsManager.EvaluateSessionPerformance();

        // each are evaluating for the next difficulty
        int _newLevel = difficultyManager.ScoreToLevel(_score);

        SetTrialLevel(_newLevel);

        difficultyEvaluationIndex++;
        Debug.Log(
            $"Increased Difficulty Evaluation to {difficultyEvaluationIndex} with new difficulty " +
            $"evaluation difficulty evaluation: {difficultyManager.difficultyName}"
        );
    }
    private void SetTrialLevel(int _newLevel)
    {
        difficultyManager.currentLevel = _newLevel;

        // TODO: This should be done by GlobalControl itself
        Debug.Log("Setting Difficulty: " + difficultyManager.currentLevel);
        GlobalControl.Instance.targetWidth = difficultyManager.hasTarget ? difficultyManager.targetWidth / 2f : 0;
        GlobalControl.Instance.timescale = difficultyManager.ballSpeed;

        targetLine.UpdateCondition();
        difficultyDisplay.text = difficultyManager.currentLevel.ToString();

        if (trialsManager.isSessionOver)
        {
            // all difficulties recored
            QuitTask();
        }
    }

    #endregion // Difficulty


    #region Feedback
    public void UpdateFeebackCanvas(TrialsManager _trialsManager)
    {
        feedbackCanvas.UpdateAllInformation(_trialsManager);
    }
    #endregion

}
