using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    #region Functions to move out of the file
    public bool isTimeOver(double elapsedTime) { return elapsedTime > currentDifficulty.maximumTrialTime; }
    public bool isSessionOver
    {
        get { return currentDifficultyChoiceIndex >= difficultyOverSession.Count; }
    }



    #region Data
    public List<TrialData> allTrialsData { get; protected set; } = new List<TrialData>();
    private TrialData currentTrial { get { return allTrialsData.Last(); } }

    public void AddBounceToCurrentResults(bool _isAccurate)
    {
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
        foreach (var trial in allTrialsData)
        {
            _totalBounces += trial.nbBounces;
            _totalAccurateBounces += trial.nbAccurateBounces;
        }
        double _averageBounces = ComputeAverage(
            _totalBounces, allTrialsData.Count, 0.0, 1.3
        );
        double _averageAccurateBounces = hasTarget ?
            ComputeAverage(_totalAccurateBounces, allTrialsData.Count, 0, 1.3) : 0;

        // evaluating time percentage of the way to end
        double _timeScalar = 1 - (_elapsedTime / currentDifficulty.maximumTrialTime);
        double _targetHeightModifier = hasTarget ? 3 : 2;
        return Mathf.Clamp01(
            (float)((_averageBounces + _averageAccurateBounces + _timeScalar) / _targetHeightModifier)
        );
    }

    public int ScoreToLevel(double _score)
    {
        return currentDifficulty.ScoreToLevel(_score);
    }
    #endregion



    #endregion



    [SerializeField, Tooltip("The paddles manager")]
    private PaddlesManager paddlesManager;

    #region Accessors
    #region Ball
    public int nbOfBounceRequired { get { return currentDifficulty.nbOfBounceRequired; } }
    public int nbOfAccurateBounceRequired { get { return currentDifficulty.nbOfAccurateBounceRequired; } }
    public float ballSpeed { get { return currentDifficulty.ballSpeed(currentLevel); } }
    #endregion

    #region Target
    public bool hasTarget { get { return currentDifficulty.hasTarget(currentLevel); } }
    public TargetEnum.Height targetHeight { get { return GlobalControl.Instance.targetHeightPreference; } }
    public float targetHeightOffset { get { return currentDifficulty.targetHeightOffset(currentLevel); } }
    public float targetWidth { get { return currentDifficulty.targetWidth(currentLevel); } }
    #endregion

    #region Trial
    public bool AreTrialConditionsMet() {
        return currentDifficulty.AreTrialConditionsMet(_currentLevel, currentTrial); 
    }
    public bool mustSwitchPaddleAfterHitting { get { return paddlesManager.NbPaddles > 1; } }
    public double maximumTrialTime { get { return currentDifficulty.maximumTrialTime; } }
    #endregion
    #endregion



    // Level is the increment inside a difficulty condition 
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



    // Difficulty is a specific set of level conditions
    #region Difficulty
    public string difficultyName { get { return currentDifficulty.name; } }
    private List<DifficultyChoice> difficultyOverSession = new List<DifficultyChoice>()
    {
        DifficultyChoice.BASE,
        DifficultyChoice.MODERATE,
        DifficultyChoice.MAXIMAL
    };
    public void AddDifficultyToSessionList(DifficultyChoice newDifficulty)
    {
        difficultyOverSession.Add(newDifficulty);
    }
    private int currentDifficultyChoiceIndex = 0;
    private DifficultyChoice currentDifficultyChoice { 
        get { return difficultyOverSession[currentDifficultyChoiceIndex];  } 
    }
    private DifficultyFactory difficultyFactory = new DifficultyFactory();
    private DifficultyDefinition currentDifficulty
    {
        get { return difficultyFactory.trialLevelDefinitions[currentDifficultyChoice]; }
    }
    #endregion
}
