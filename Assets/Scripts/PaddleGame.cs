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
    // Manage the current task to perform
    [SerializeField, Tooltip("The main trial manager for the game")]
    private TrialsManager trialsManager;
    
    [Tooltip("The ball being bounced")]
    [SerializeField]
    private GameObject ball;

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

    [SerializeField]
    private GlobalPauseHandler pauseHandler;

    // Degrees of freedom, how many degrees in x-z directions ball can bounce after hitting paddle
    // 0 degrees: ball can only bounce in y direction, 90 degrees: no reduction in range
    public float degreesOfFreedom;

    float difficultyExampleTime = 30f;

    void Start()
    {
        // Load the visual environment
        Instantiate(GlobalControl.Instance.environments[GlobalControl.Instance.environmentIndex]);


        trialsManager.ChangeLevel(GlobalControl.Instance.level);
        Initialize(true);


        // difficulty shifts timescale, so pause it again
        pauseHandler.Pause();
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

    #region UI Manager
    public void UpdateFeebackCanvas(TrialsManager _trialsManager)
    {
        feedbackCanvas.UpdateAllInformation(_trialsManager);
    }

    public void ToggleTimerCountdownCanvas(bool value)
    {
        timeToDropQuad.SetActive(value);
    }
    #endregion


    #region Ball effect manager
    public void TriggerBallRespawn(bool spawnOnly)
    {
        StartCoroutine(ball.GetComponent<Ball>().RespawningCoroutine(pauseHandler, spawnOnly));
    }

    public IEnumerator ManageCountdownToDropCanvasCoroutine(int countdownTime)
    {
        ToggleTimerCountdownCanvas(true);
        while (countdownTime >= 1.0f)
        {
            timeToDropText.text = countdownTime.ToString();
            countdownTime--;
            yield return new WaitForSeconds(1.0f);
        }
        ToggleTimerCountdownCanvas(false);
    }
    #endregion

    #region Level
    public void UpdateCurrentLevelText(int _newLevel)
    {
        difficultyDisplay.text = trialsManager.currentLevel.ToString();
    }
    #endregion









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
            difficultyDisplay.text = trialsManager.currentLevel.ToString();
        }
        else if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
        {
            trialsManager.ChangeLevel(2);
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
    /// run through all difficulties in a short amount of time to get a feel for them
    /// </summary>
    void StartShowcase()
    {
        pauseHandler.Resume();
        StartCoroutine(StartDifficultyDelayedCoroutine(difficultyExampleTime, true));
    }

    IEnumerator StartDifficultyDelayedCoroutine(float delay, bool initial = false)
    {
        if (initial)
        {
            // wait until after the pause is lifted, when timescale is 0
            yield return new WaitForSeconds(.1f);
        }

        var audioClip = GetDifficultyAudioClip(trialsManager.currentLevel);
        if (audioClip != null)
        {
            difficultySource.PlayOneShot(audioClip);
        }
        yield return new WaitForSecondsRealtime(delay);

        int _newLevel = trialsManager.currentLevel + 2;
        if (_newLevel > 10)
            // finish up the difficulty showcase, quit application
            QuitTask();

        // reset ball, change difficulty level, possible audio announcement.
        trialsManager.ChangeLevel(_newLevel);
        StartCoroutine(StartDifficultyDelayedCoroutine(difficultyExampleTime));
        ball.GetComponent<Ball>().ResetBall();
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
        // TODO: DO SOME COMPUTATION
        double _score = trialsManager.EvaluateSessionPerformance();
        int _newLevel = 1;

        trialsManager.ChangeLevel(_newLevel);
    }

    #endregion // Difficulty


}
