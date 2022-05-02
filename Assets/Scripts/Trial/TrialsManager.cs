using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrialsManager : MonoBehaviour
{
    [SerializeField]
    private DifficultyManager difficultyManager;

    private void Start()
    {

        if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
        {
            difficultyManager.AddDifficultyToSessionList(DifficultyChoice.BASE);
            difficultyManager.AddDifficultyToSessionList(DifficultyChoice.MODERATE);
            difficultyManager.AddDifficultyToSessionList(DifficultyChoice.MAXIMAL);
        } else
        {
            difficultyManager.AddDifficultyToSessionList(GlobalControl.Instance.practiseDifficulty);
        }
    }


    #region One specific trial
    private Trial currentTrial { get { return allTrialsData.Last(); } }
    public void AddBounceToCurrentTrial(bool _isAccurate)
    {
        currentTrial.AddBounce(_isAccurate);
    }
    public bool CheckIfTrialIsOver()
    {
        if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
            return false;

        return difficultyManager.AreTrialConditionsMet(currentTrial);
    }
    #endregion

    #region All trials
    public bool isTimeOver(double elapsedTime) { return elapsedTime > difficultyManager.maximumTrialTime; }
    public bool isSessionOver { get { return difficultyManager.AreAllDifficultiesDone; } }
    private List<Trial> allTrialsData = new List<Trial>();
    public void StartNewTrial() { allTrialsData.Add(new Trial(GlobalControl.Instance.GetTimeElapsed())); }

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
                (float)(_average / difficultyManager.nbOfBounceRequired),
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
        double _averageAccurateBounces = difficultyManager.hasTarget ?
            ComputeAverage(_totalAccurateBounces, allTrialsData.Count, 0, 1.3) : 0;

        // evaluating time percentage of the way to end
        double _timeScalar = 1 - (_elapsedTime / difficultyManager.maximumTrialTime);
        double _targetHeightModifier = difficultyManager.hasTarget ? 3 : 2;
        return Mathf.Clamp01(
            (float)((_averageBounces + _averageAccurateBounces + _timeScalar) / _targetHeightModifier)
        );
    }
    #endregion
}
