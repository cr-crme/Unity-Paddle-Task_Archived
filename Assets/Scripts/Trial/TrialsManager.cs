using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrialsManager : MonoBehaviour
{
    [SerializeField]
    private DifficultyManager difficultyManager;
    private DynamicDifficultyAlgorithm dda = new DynamicDifficultyAlgorithm();

    [SerializeField]
    private PaddleGame uiManager;

    [SerializeField]
    [Tooltip("The paddles in the game")]
    private PaddlesManager paddlesManager;

    [SerializeField]
    private Ball ball;

    [SerializeField]
    private Target target;

    public bool isPreparingNewTrial { get; private set; } = true;

    private void Start()
    {
        bestSoFarNbOfBounces = 0;
        StartSession();
    }

    private void Update()
    {
        if (isPreparingNewTrial) return;

        ManageIfEndOfTrial();
    }

    #region Current trial in the session
    public void StartNewTrial()
    {
        IEnumerator FinalizeStartNewTrialCoroutine()
        {
            yield return new WaitWhile(() => ball.inRespawnMode);
            isPreparingNewTrial = false;
        }
        if (allTrialsData.Count > 0 && allTrialsData.Last().nbBounces > bestSoFarNbOfBounces)
        {
            bestSoFarNbOfBounces = allTrialsData.Last().nbBounces;
        }
        allTrialsData.Add(new Trial(GlobalControl.Instance.elapsedTime));
        trialAtChangeOfLevel = new Trial(GlobalControl.Instance.elapsedTime);

        uiManager.TriggerBallRespawn(allTrialsData.Count == 1);
        uiManager.UpdateFeebackCanvas(this);
        StartCoroutine(FinalizeStartNewTrialCoroutine());
    }
    public void ManageIfEndOfTrial(bool forceEndOfTrial = false)
    {
        IEnumerator FinalizeTrialCoroutine()
        {
            yield return new WaitWhile(() => !ball.isOnGround);

            StartNewTrial();
            if (isSessionOver)
            {
                isPreparingNewTrial = false;
                uiManager.QuitTask();
            }
        }

        if (isPreparingNewTrial || ball.inRespawnMode || ball.inHoverMode) 
            return;

        if (forceEndOfTrial)
            ball.ForceToDrop();

        if (ball.isOnGround || isSessionOver)
        {
            isPreparingNewTrial = true;
            StartCoroutine(FinalizeTrialCoroutine());
        }
    }
    public void ForceEndOfTrial()
    {
        if (isPreparingNewTrial) return;

        ManageIfEndOfTrial(true);
    }
    private Trial currentTrial { get { return allTrialsData.Last(); } }
    private float trialTime { get { return GlobalControl.Instance.elapsedTime - currentTrial.time; } }
    private float maximumTrialTime { 
        get
        {
            if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
            {
                return GlobalControl.Instance.showcaseTimePerCondition * 60f * difficultyManager.nbLevel;
            }
            else if (GlobalControl.Instance.session == SessionType.Session.PRACTISE)
            {
                return GlobalControl.Instance.practiseMaxTrialTime * 60f;
            }
            else
            {
                Debug.LogError("Time over not implemented for current session type");
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
    public int currentNumberOfBounces { get { return currentTrial.nbBounces; } }
    public int currentNumberOfAccurateBounces { get { return currentTrial.nbAccurateBounces; } }
    public int currentLevel { get { return difficultyManager.currentLevel; } }
    public void ForceLevelChanging(int _newLevel)
    {
        trialAtChangeOfLevel = new Trial(GlobalControl.Instance.elapsedTime);
        difficultyManager.SetCurrentLevel(_newLevel);
        target.UpdateCondition();
        uiManager.UpdateCurrentLevelText(_newLevel);
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
        if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
            return false;

        return difficultyManager.AreTrialConditionsMet(currentTrial);
    }
    public bool hasTarget { get { return difficultyManager.hasTarget; } }
    #endregion



    #region Full Session
    public void StartSession()
    {
        _sessionTime = GlobalControl.Instance.elapsedTime;
    }
    private float _sessionTime;
    public float sessionTime { get { return GlobalControl.Instance.elapsedTime - _sessionTime; } }
    public bool isSessionOver {
        get
        {
            float maxTime = maximumTrialTime;
            if (maxTime <= 0) return false;
            else return trialTime > maxTime; 
        } 
    }
    private List<Trial> allTrialsData = new List<Trial>();
    Trial trialAtChangeOfLevel;
    public int bestSoFarNbOfBounces;
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
        double _timeScalar = 1 - (GlobalControl.Instance.elapsedTime / maximumTrialTime);
        double _targetHeightModifier = difficultyManager.hasTarget ? 3 : 2;
        return Mathf.Clamp01(
            (float)((_averageBounces + _averageAccurateBounces + _timeScalar) / _targetHeightModifier)
        );
    }
    #endregion
}
