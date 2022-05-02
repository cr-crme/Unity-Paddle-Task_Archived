using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Delegate function to evaluate the performance of a Trial (series of bounces)
/// according to determined criteria of level and difficulty
/// </summary>
public delegate bool IsTrialSuccessfulEvaluator(
    int currentLevel, TrialData _trialData, DifficultyDefinition _difficultyDefinition
);

public class RandomizableFloat
{
    public RandomizableFloat(float _val, float _mod = 0)
    {
        val = _val;
        mod = _mod;
        randomize = _mod != 0;
    }

    bool randomize;
    float val;
    float mod;
    public float value { get { return randomize ? UnityEngine.Random.Range(val - mod, val + mod) : val; } }

}

public class DifficultyDefinition
{
    public DifficultyDefinition(
        string _name,
        double _maximumTrialTime,
        int _nbOfBounceRequired,
        int _nbOfAccurateBounceRequired,
        bool _shouldBeSequentialBounces,
        Tuple<double, double> _baseScoreBonus,
        List<bool> _showTargetByLevel,
        List<RandomizableFloat> _targetHeightModifierByLevel,
        List<RandomizableFloat> _ballSpeedByLevel,
        List<RandomizableFloat> _targetWidthByLevel,
        IsTrialSuccessfulEvaluator _trialPerformanceEvaluator
    )
    {
        name = _name;
        maximumTrialTime = _maximumTrialTime;
        nbOfBounceRequired = _nbOfBounceRequired;
        nbOfAccurateBounceRequired = _nbOfAccurateBounceRequired;
        shouldBeSequentialBounces = _shouldBeSequentialBounces;
        showTargetByLevel = _showTargetByLevel;
        baseScoreBonus = _baseScoreBonus;
        targetHeightOffsetByLevel = _targetHeightModifierByLevel;
        ballSpeedByLevel = _ballSpeedByLevel;
        targetWidthByLevel = _targetWidthByLevel;

        if (
            showTargetByLevel.Count != targetHeightOffsetByLevel.Count ||
            showTargetByLevel.Count != ballSpeedByLevel.Count || 
            showTargetByLevel.Count != targetWidthByLevel.Count
        )
        {
            Debug.LogError("Wrong number of element in the current difficulty");
        }
        isTrialSuccessful = _trialPerformanceEvaluator;
    }
    public string name { get; private set; }
    public int nbLevel { get { return showTargetByLevel.Count; } }
    public int nbOfBounceRequired { get; private set; }
    public int nbOfAccurateBounceRequired { get; private set; }
    public bool shouldBeSequentialBounces { get; private set; }
    public double maximumTrialTime { get; private set; }

    List<bool> showTargetByLevel;
    public bool hasTarget(int _currentLevel) { 
        return showTargetByLevel[_currentLevel]; 
    }

    public Tuple<double, double> baseScoreBonus { get; protected set; }
    private List<RandomizableFloat> targetHeightOffsetByLevel;
    public float targetHeightOffset(int _currentLevel) { return targetHeightOffsetByLevel[_currentLevel].value; }

    private List<RandomizableFloat> targetWidthByLevel;
    public float targetWidth(int _currentLevel){ return targetWidthByLevel[_currentLevel].value; }

    private List<RandomizableFloat> ballSpeedByLevel;
    public float ballSpeed(int _currentLevel) { return ballSpeedByLevel[_currentLevel].value; }

    public int ScoreToLevel(double _normalizedScore)
    {
        float _lerpedScore = Mathf.Lerp(
            (float)baseScoreBonus.Item1, (float)baseScoreBonus.Item2, (float)_normalizedScore
        );
        float _lerpedClampedScore = Mathf.Clamp(_lerpedScore, 0, nbLevel);
        return Mathf.RoundToInt(_lerpedClampedScore);
    }

    private IsTrialSuccessfulEvaluator isTrialSuccessful;
    public bool AreTrialConditionsMet(int _currentLevel, TrialData _trialData) {
        return isTrialSuccessful(_currentLevel, _trialData, this); 
    }
}
