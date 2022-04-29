using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    [SerializeField, Tooltip("The paddles manager")]
    private PaddlesManager paddlesManager;

    #region Interface
    public int nbOfBounceRequired { get { return currentDifficulty.nbOfBounceRequired; } }
    public int nbOfAccurateBounceRequired { get { return currentDifficulty.nbOfAccurateBounceRequired; } }
    public bool hasTarget { get { return currentDifficulty.shouldShowTarget(currentLevel); } }
    public SessionType.TargetHeight targetHeight { get { return GlobalControl.Instance.targetHeightPreference; } }
    public float targetHeightOffset { get { return currentDifficulty.targetHeightOffset(currentLevel); } }
    public float targetWidth { get { return currentDifficulty.targetWidth(currentLevel); } }
    public float ballSpeed { get { return currentDifficulty.ballSpeed(currentLevel); } }
    public bool isTimeOver(double elapsedTime) { return elapsedTime > maximumTrialTime; }
    public bool isSessionOver
    {
        get { return currentDifficultyChoiceIndex >= difficultyOverSession.Count; }
    }
    public double maximumTrialTime { get { return currentDifficulty.maximumTrialTime; } }
    public bool mustSwitchPaddleAfterHitting { get { return paddlesManager.NbPaddles > 1; } }
    #endregion



    #region Level
    private int _currentLevel = 0;
    public int currentLevel { 
        get { return _currentLevel; }
        set
        {
            if (value < 0 || value > currentDifficulty.nbLevel)
            {
                Debug.LogError("Issue setting difficulty, not in expected range: " + value);
                return;
            }
            _currentLevel = value;
        }
    }
    #endregion



    #region Difficulty
    public string difficultyName { get { return currentDifficulty.name; } }
    List<DifficultyChoice> difficultyOverSession = new List<DifficultyChoice>()
    {
        DifficultyChoice.BASE,
        DifficultyChoice.MODERATE,
        DifficultyChoice.MAXIMAL,
        DifficultyChoice.MODERATE
    };
    public void AddDifficultyToSessionList(DifficultyChoice newDifficulty)
    {
        difficultyOverSession.Add(newDifficulty);
    }
    int currentDifficultyChoiceIndex = 0;
    DifficultyChoice currentDifficultyChoice { 
        get { return difficultyOverSession[currentDifficultyChoiceIndex];  } 
    }
    DifficultyFactory difficultyFactory = new DifficultyFactory();
    DifficultyDefinition currentDifficulty
    {
        get { return difficultyFactory.trialLevelDefinitions[currentDifficultyChoice]; }
    }
    #endregion

    #region Data
    List<TrialResults> trialResults = new List<TrialResults>();
    TrialResults currentTrial { get { return trialResults.Last(); } }
    public void AddBounceToCurrentResults(bool _isAccurate) { 
        currentTrial.AddBounce(_isAccurate); 
    }
    public double EvaluateSessionPerformance(
        double _elapsedTime
    )
    {
        double ComputeAverage(
            double _total, double _nbElement, double _minValue, double _maxValue
        )
        {
            // Computed bounded average of the bouncing and accurate bouncing
            double _average = _total / _nbElement;
            _average = double.IsNaN(_average) ? 0 : _average;
            return (double)Mathf.Clamp(
                (float)(_average / currentDifficulty.nbOfBounceRequired),
                (float)_minValue,
                (float)_maxValue
            );
        }

        int _totalBounces = 0, _totalAccurateBounces = 0;
        foreach (var trial in trialResults)
        {
            _totalBounces += trial.nbBounces;
            _totalAccurateBounces += trial.nbAccurateBounces;
        }
        double _averageBounces = ComputeAverage(
            _totalBounces, trialResults.Count, 0.0, 1.3
        );
        double _averageAccurateBounces = hasTarget ? 
            ComputeAverage(_totalAccurateBounces, trialResults.Count, 0, 1.3) : 0;

        // evaluating time percentage of the way to end
        double _timeScalar = 1 - (_elapsedTime / currentDifficulty.maximumTrialTime);
        double _targetHeightModifier = hasTarget ? 3 : 2;
        return Mathf.Clamp01(
            (float)((_averageBounces + _averageAccurateBounces + _timeScalar) / _targetHeightModifier)
        );
    }

    public double EvaluatePerformance(double elapsedTime)
    {
        return EvaluateSessionPerformance(elapsedTime);
    }
    public int ScoreToLevel(double _score)
    {
        return currentDifficulty.ScoreToLevel(_score);
    }
    #endregion

}
