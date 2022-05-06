using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{	
    [Tooltip("The ball being bounced")]
    [SerializeField]
    private Ball ball;

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
    private TextMeshPro difficultyDisplay;

    /// <summary>
    /// list of the audio clips played at the beginning of difficulties in some cases
    /// </summary>
    private AudioManager audioManager;
    private GlobalPauseHandler pauseHandler;

    float difficultyExampleTime = 30f;

    #region Initialization
    void Start()
    {
        audioManager = GetComponent<AudioManager>();
        pauseHandler = GetComponent<GlobalPauseHandler>();

        // Load the visual environment
        Instantiate(GlobalControl.Instance.environments[GlobalControl.Instance.environmentIndex]);
        // difficulty shifts timescale, so pause it again
        pauseHandler.Pause();
    }

    void OnApplicationQuit()
    {
        QuitTask(GetComponent<TrialsManager>());
    }

    /// <summary>
    /// Stop the task, write data and return to the start screen
    /// </summary>
    public void QuitTask(TrialsManager _trialsManager)
    {
        IEnumerator QuitWhenTrialIsProcessed()
        {
            yield return new WaitUntil(() => (_trialsManager.isPreparingNewTrial));

            // clean DDoL objects and return to the start scene
            Destroy(GlobalControl.Instance.gameObject);
            Destroy(gameObject);

            SceneManager.LoadScene(0);
        }

        // This is to ensure that the final trial is recorded.
        _trialsManager.ForceEndOfTrial();
        StartCoroutine(QuitWhenTrialIsProcessed());
    }
    #endregion

    #region UI Manager
    public void UpdateFeebackCanvas(TrialsManager _trialsManager)
    {
        feedbackCanvas.UpdateAllInformation(_trialsManager);
    }

    public void ToggleTimerCountdownCanvas(bool _value)
    {
        timeToDropQuad.SetActive(_value);
    }

    public void UpdateLevel(int _newLevel)
    {
        difficultyDisplay.text = _newLevel.ToString();
        audioManager.PlayDifficultyAudioClip(_newLevel);
    }
    #endregion

    #region Ball effect manager
    public void TriggerBallRespawn(bool spawnOnly)
    {
        StartCoroutine(ball.RespawningCoroutine(pauseHandler, spawnOnly));
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









    #region Initialization

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

        //var audioClip = GetDifficultyAudioClip(trialsManager.currentLevel);
        //if (audioClip != null)
        //{
        //    difficultySource.PlayOneShot(audioClip);
        //}
        yield return new WaitForSecondsRealtime(delay);

        //int _newLevel = trialsManager.currentLevel + 2;
        //if (_newLevel >= 10)
        //    // finish up the difficulty showcase, quit application
        //    QuitTask();

        //// reset ball, change difficulty level, possible audio announcement.
        //trialsManager.ForceLevelChanging(_newLevel);
        StartCoroutine(StartDifficultyDelayedCoroutine(difficultyExampleTime));
        ball.ResetBall();
    }


    #endregion // Reset

    #region Checks, Interactions, Data

    #endregion // Checks, Interactions, Data

}
