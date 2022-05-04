using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrialsManager : MonoBehaviour
{
    [SerializeField]
    private DifficultyManager difficultyManager;

    [SerializeField]
    private PaddleGame paddleGame;

    [SerializeField]
    [Tooltip("The paddles in the game")]
    private PaddlesManager paddlesManager;

    [SerializeField]
    private Ball ball;

    public bool isPreparingNewTrial { get; private set; } = true;

    private void Awake()
    {
        bestSoFarNbOfBounces = 0;

        difficultyManager = GetComponent<DifficultyManager>();
        if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
        {
            difficultyManager.AddDifficultyToSessionList(DifficultyChoice.BASE);
            difficultyManager.AddDifficultyToSessionList(DifficultyChoice.MODERATE);
            difficultyManager.AddDifficultyToSessionList(DifficultyChoice.MAXIMAL);
        } 
        else
        {
            difficultyManager.AddDifficultyToSessionList(GlobalControl.Instance.practiseDifficulty);
            difficultyManager.AddDifficultyToSessionList(GlobalControl.Instance.practiseDifficulty);
        }
        StartNewSession();
    }

    private void Update()
    {
        if (isPreparingNewTrial) return;

        ManageIfEndOfTrial();
    }

    #region Current trial in the session
    public void StartNewTrial()
    {
        if (allTrialsData.Count > 0 && allTrialsData.Last().nbBounces > bestSoFarNbOfBounces)
        {
            bestSoFarNbOfBounces = allTrialsData.Last().nbBounces;
        }
        allTrialsData.Add(new Trial(GlobalControl.Instance.elapsedTime));
        isPreparingNewTrial = false;
    }
    public void ManageIfEndOfTrial(bool forceEndOfTrial = false)
    {
        IEnumerator FinalizeTrialCoroutine()
        {
            yield return new WaitWhile(() => !ball.isOnGround());
            Debug.Log($"Trial is over the session performance score is {EvaluateSessionPerformance()}");

            StartNewTrial();
            if (isSessionOver)
            {
                isPreparingNewTrial = false;
                paddleGame.QuitTask();
            }
        }

        if (isPreparingNewTrial) return;

        if (forceEndOfTrial || isTrialTimeOver() || AreTrialConditionsMet())
        {
            isPreparingNewTrial = true;
            ball.ForceToDrop();
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
    private bool isTrialTimeOver()
    {
        float maxTime = maximumTrialTime;
        if (maxTime <= 0) return false;
        else return trialTime > maxTime;
    }
    private float maximumTrialTime { 
        get
        {
            if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
            {
                return GlobalControl.Instance.showcaseTimePerCondition * 60f;
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
        paddleGame.UpdateFeebackCanvas(this);
        paddlesManager.SwitchPaddleIfNeeded(difficultyManager);

        if (isSessionOver)
        {
            paddleGame.QuitTask();
        }
    }
    public void AddAccurateBounceToCurrentTrial()
    {
        currentTrial.AddAccurateBounce();
    }
    public int currentNumberOfBounces { get { return currentTrial.nbBounces; } }
    public int currentNumberOfAccurateBounces { get { return currentTrial.nbAccurateBounces; } }
    public bool AreTrialConditionsMet()
    {
        if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
            return false;

        return difficultyManager.AreTrialConditionsMet(currentTrial);
    }
    public bool hasTarget { get { return difficultyManager.hasTarget; } }
    #endregion



    #region Full Session
    public void StartNewSession()
    {
        difficultyManager.ProceedToNextDifficulty();
        _sessionTime = GlobalControl.Instance.elapsedTime;
    }
    private float _sessionTime;
    public float sessionTime { get { return GlobalControl.Instance.elapsedTime - _sessionTime; } }
    public bool isSessionOver { get { return difficultyManager.AreAllDifficultiesDone; } }
    private List<Trial> allTrialsData = new List<Trial>();
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
