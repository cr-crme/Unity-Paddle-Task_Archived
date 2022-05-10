using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrialsManager : MonoBehaviour
{
    private DifficultyManager difficultyManager;
    private DynamicDifficultyAlgorithm dda = new DynamicDifficultyAlgorithm();

    private UiManager uiManager;

    [SerializeField]
    [Tooltip("The paddles in the game")]
    private PaddlesManager paddlesManager;

    [SerializeField]
    private Ball ball;

    [SerializeField]
    private Target target;
    public bool isInActiveTrial { get; private set; } = false;

    private SaveTrialManager saveTrialManager = new SaveTrialManager();

    private void Start()
    {
        difficultyManager = GetComponent<DifficultyManager>();
        uiManager = GetComponent<UiManager>();

        bestSoFarNbOfBounces = 0;
        StartSession();

        if (GlobalPreferences.Instance.session == SessionType.Session.SHOWCASE)
        {
            ForceLevelChanging(2);
        }
        else if (GlobalPreferences.Instance.session == SessionType.Session.PRACTISE)
        {
            ForceLevelChanging(GlobalPreferences.Instance.practiseStartingLevel);
        }
        else if (GlobalPreferences.Instance.session == SessionType.Session.TUTORIAL)
        {
            ForceLevelChanging(0);
            GlobalPreferences.Instance.SetPlayVideo(true);
        }
        else
        {
            Debug.LogError($"SessionType: {GlobalPreferences.Instance.session} not implemented yet");
        }

        IEnumerator WaitNextFrameAndStartFirstTrial()
        {
            // Wait for a couple of frames that everything is loaded and ready then start a trial
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            StartNewTrial();
        }
        StartCoroutine(WaitNextFrameAndStartFirstTrial());
    }

    private void Update()
    {
        if (!isInActiveTrial) return;

        ManageIfEndOfTrial();
    }

    private void FixedUpdate()
    {
        if (!isInActiveTrial) return;

        // saveTrialManager.AddFrameToTrial(currentTrial, ball);
    }

    #region Current trial in the session
    public void StartNewTrial()
    {
        IEnumerator FinalizeStartNewTrialCoroutine()
        {
            yield return new WaitWhile(() => ball.inRespawnMode);
            GlobalPreferences globalControl = GlobalPreferences.Instance;
            allTrialsData.Add(new Trial(Time.time + globalControl.ballResetHoverSeconds));
            trialAtChangeOfLevel = new Trial(Time.time + globalControl.ballResetHoverSeconds);
            uiManager.UpdateFeebackCanvas(this);
            isInActiveTrial = true;
        }
        if (allTrialsData.Count > 0 && allTrialsData.Last().nbBounces > bestSoFarNbOfBounces)
        {
            bestSoFarNbOfBounces = allTrialsData.Last().nbBounces;
        }
        uiManager.TriggerBallRespawn(allTrialsData.Count == 0);
        StartCoroutine(FinalizeStartNewTrialCoroutine());
    }
    public void ManageIfEndOfTrial()
    {
        if (!isInActiveTrial || ball.inRespawnMode || ball.inHoverMode) 
            return;

        if (ball.isOnGround || isSessionOver)
        {
            StartCoroutine(FinalizeTrialCoroutine(true));
        }
    }
    public void ForceEndOfTrial(bool _startNewTrial = true)
    {
        if (!isInActiveTrial) return;

        ball.ForceToDrop();
        StartCoroutine(FinalizeTrialCoroutine(_startNewTrial));
    }
    IEnumerator FinalizeTrialCoroutine(bool _startNewTrial)
    {
        isInActiveTrial = false;
        yield return new WaitWhile(() => !ball.isOnGround);
        if (isSessionOver)
        {
            isInActiveTrial = true;  // Trick the Quit so it records everything
            uiManager.QuitTask(this);
        }
        if (_startNewTrial)
            StartNewTrial();
    }

    private Trial currentTrial { get { return allTrialsData.Count == 0 ? null : allTrialsData.Last(); } }
    private float maximumTrialTime { 
        get
        {
            GlobalPreferences globalControl = GlobalPreferences.Instance;
            if (globalControl.session == SessionType.Session.SHOWCASE)
            {
                
                return globalControl.showcaseTimePerCondition 
                    * globalControl.timeConversionToMinute 
                    * ( difficultyManager.nbLevel / 2 );  // Show case is moving two at a time
            }
            else if (globalControl.session == SessionType.Session.PRACTISE)
            {
                return globalControl.practiseMaxTrialTime * globalControl.timeConversionToMinute;
            }
            else if (globalControl.session == SessionType.Session.TUTORIAL)
            {
                return 0;
            }
            else
            {
                Debug.LogError($"SessionType: {GlobalPreferences.Instance.session} not implemented yet");
                return 0;
            }
        }
    }
    public void AddBounceToCurrentTrial()
    {
        currentTrial.AddBounce();
        trialAtChangeOfLevel.AddBounce();

        uiManager.UpdateFeebackCanvas(this);
        paddlesManager.SwitchPaddleIfNeeded(difficultyManager);

        AutomaticLevelChanging();
    }
    public void AddAccurateBounceToCurrentTrial()
    {
        currentTrial.AddAccurateBounce();
        trialAtChangeOfLevel.AddAccurateBounce();

        uiManager.UpdateFeebackCanvas(this);
    }
    public int currentNumberOfBounces { get { return currentTrial == null ? 0 : currentTrial.nbBounces; } }
    public int currentNumberOfAccurateBounces { get { return currentTrial == null ? 0 : currentTrial.nbAccurateBounces; } }
    public int currentLevel { get { return difficultyManager.currentLevel; } }
    public void ForceLevelChanging(int _newLevel)
    {
        trialAtChangeOfLevel = new Trial(Time.time);
        difficultyManager.SetCurrentLevel(_newLevel);
        ball.UpdatePhysics(difficultyManager);
        target.UpdateCondition(this);
        uiManager.UpdateLevel(_newLevel);
    }
    public void AutomaticLevelChanging()
    {
        int _newLevel = dda.ComputeNewLevel(difficultyManager, trialAtChangeOfLevel);
        if (_newLevel < 0)
            return;

        ForceLevelChanging(_newLevel);
    }
    public bool AreTrialConditionsMet()
    {
        if (GlobalPreferences.Instance.session == SessionType.Session.SHOWCASE)
            return false;

        return difficultyManager.AreTrialConditionsMet(currentTrial);
    }
    public bool hasTarget { get { return difficultyManager.hasTarget; } }
    public TargetEnum.Height targetBaseHeight { get { return difficultyManager.targetBaseHeight; } }
    public double targetHeightOffset { get { return difficultyManager.targetHeightOffset; } }
    public double targetWidth { get { return difficultyManager.targetWidth; } }
    #endregion



    #region Full Session
    public void StartSession()
    {
        GlobalPreferences globalControl = GlobalPreferences.Instance;
        _sessionTime = Time.time + globalControl.ballResetHoverSeconds;
    }
    private float _sessionTime;
    public float sessionTime { get { return Time.time - _sessionTime; } }
    public bool isSessionOver {
        get
        {
            float maxTime = maximumTrialTime;
            if (maxTime <= 0) return false;
            else return sessionTime > maxTime; 
        } 
    }
    private List<Trial> allTrialsData = new List<Trial>();
    Trial trialAtChangeOfLevel;
    public int bestSoFarNbOfBounces { get; private set; }
    public double EvaluateSessionPerformance()
    {
        double ComputeAverage(
            double _total, double _nbElement, double _minValue, double _maxValue
        )
        {
            // Computed bounded average of the bouncing and accurate bouncing
            double _average = _total / _nbElement;
            _average = double.IsNaN(_average) ? 0 : _average;
            return (double)Mathf.Clamp(
                (float)(_average / difficultyManager.nbOfBounceRequired),
                (float)_minValue,
                (float)_maxValue
            );
        }

        int _totalBounces = 0;
        int _totalAccurateBounces = 0;
        foreach (var trial in allTrialsData)
        {
            _totalBounces += trial.nbBounces;
            _totalAccurateBounces += trial.nbAccurateBounces;
        }
        double _averageBounces = ComputeAverage(_totalBounces, allTrialsData.Count, 0.0, 1.3);
        double _averageAccurateBounces = difficultyManager.hasTarget ?
            ComputeAverage(_totalAccurateBounces, allTrialsData.Count, 0, 1.3) : 0;

        // evaluating time percentage of the way to end of the session
        double _timeScalar = 1 - (Time.time / maximumTrialTime);
        double _targetHeightModifier = difficultyManager.hasTarget ? 3 : 2;
        return Mathf.Clamp01(
            (float)((_averageBounces + _averageAccurateBounces + _timeScalar) / _targetHeightModifier)
        );
    }
    #endregion
}
